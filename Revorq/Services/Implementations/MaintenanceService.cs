using Revorq.API.Models;
using Revorq.API.Models.MaintenanceOrderModels;
using Revorq.API.Services.Interfaces;
using Revorq.DAL.Entities;
using Revorq.DAL.Enums;
using Revorq.DAL.Repositories.Interfaces;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using Microsoft.AspNetCore.Identity;

namespace Revorq.API.Services.Implementations;

public class MaintenanceService : IMaintenanceService
{
    private readonly IMaintenanceOrderRepository _orderRepository;
    private readonly IMaintenanceReportRepository _reportRepository;
    private readonly IElevatorRepository _elevatorRepository;
    private readonly UserManager<AppUser> _userManager;

    public MaintenanceService(
        IMaintenanceOrderRepository orderRepository,
        IMaintenanceReportRepository reportRepository,
        IElevatorRepository elevatorRepository,
        UserManager<AppUser> userManager)
    {
        _orderRepository = orderRepository;
        _reportRepository = reportRepository;
        _elevatorRepository = elevatorRepository;
        _userManager = userManager;
    }

    public async Task<IEnumerable<MaintenanceOrderResponse>> GetOrdersUntilDateAsync(DateTime untilDate)
    {
        var orders = await _orderRepository.GetOrdersUntilDateAsync(untilDate);
        return orders.Select(MapToResponse);
    }

    public async Task<IEnumerable<MaintenanceOrderResponse>> GetMonthlyAsync(int userId, int year, int month, OrderStatus? status, bool? isUnassigned, bool? isScheduled)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
            return [];

        var roles = await _userManager.GetRolesAsync(user);
        int? assignedEngineerId = roles.Contains(nameof(Role.MaintenanceEngineer)) ? userId : null;

        var orders = await _orderRepository.GetMonthlyOrdersAsync(userId, assignedEngineerId, year, month, status, isUnassigned, isScheduled);
        return orders.Select(MapToResponse);
    }

    public async Task<IEnumerable<MaintenanceOrderResponse>> GetUnscheduledAsync()
    {
        var orders = await _orderRepository.GetUnscheduledOrdersAsync();
        return orders.Select(MapToResponse);
    }

    public async Task<ServiceResult<int>> CreateOrderAsync(CreateOrderRequest request)
    {
        var elevator = await _elevatorRepository.GetByIdAsync(request.ElevatorId);
        if (elevator is null)
            return ServiceResult<int>.Error($"Elevator {request.ElevatorId} not found.");

        var order = new MaintenanceOrder
        {
            ElevatorId = request.ElevatorId,
            AssignedEngineerId = request.AssignedEngineerId,
            MaintenanceType = request.MaintenanceType,
            ScheduledDate = request.ScheduledDate,
            ShortDescription = request.ShortDescription,
            Status = OrderStatus.Open
        };

        await _orderRepository.AddAsync(order);
        await _orderRepository.SaveChangesAsync();

        return ServiceResult<int>.Ok(order.Id);
    }

    public async Task<ServiceResult<MaintenanceOrderResponse>> GetByIdAsync(int id)
    {
        var order = await _orderRepository.GetByIdWithReportAsync(id);
        if (order is null)
            return ServiceResult<MaintenanceOrderResponse>.NotFound($"Order {id} not found.");

        return ServiceResult<MaintenanceOrderResponse>.Ok(MapToResponse(order));
    }

    public async Task<ServiceResult<bool>> AssignOrderAsync(int orderId, int engineerId)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);
        if (order is null)
            return ServiceResult<bool>.NotFound($"Order {orderId} not found.");

        if (order.Status == OrderStatus.Done)
            return ServiceResult<bool>.Error("Cannot assign a completed order.");

        order.AssignedEngineerId = engineerId;

        _orderRepository.Update(order);
        await _orderRepository.SaveChangesAsync();

        return ServiceResult<bool>.Ok(true);
    }

    public async Task<ServiceResult<bool>> CreateReportAsync(int orderId, CreateReportRequest request)
    {
        var order = await _orderRepository.GetByIdWithReportAsync(orderId);
        if (order is null)
            return ServiceResult<bool>.NotFound($"Order {orderId} not found.");

        if (order.Status == OrderStatus.Done)
            return ServiceResult<bool>.Error("Order is already completed.");

        byte[]? imageData = null;
        if (request.Image is not null)
        {
            using var image = await Image.LoadAsync(request.Image.OpenReadStream());
            image.Mutate(x => x.Resize(new ResizeOptions
            {
                Size = new Size(1024, 1024),
                Mode = ResizeMode.Max
            }));
            using var ms = new MemoryStream();
            await image.SaveAsJpegAsync(ms);
            imageData = ms.ToArray();
        }

        if (order.Report is null)
        {
            var report = new MaintenanceReport
            {
                OrderId = orderId,
                JobStartedDate = request.JobStartedDate,
                CompletedDate = request.CompletedDate,
                IssueDetected = request.IssueDetected,
                VisualCheckDone = request.VisualCheckDone,
                AdjustmentDone = request.AdjustmentDone,
                CleaningDone = request.CleaningDone,
                ShortDescription = request.ShortDescription,
                ImageData = imageData
            };

            await _reportRepository.AddAsync(report);
        }
        else
        {
            var report = order.Report;
            report.JobStartedDate = request.JobStartedDate;
            report.CompletedDate = request.CompletedDate;
            report.IssueDetected = request.IssueDetected;
            report.VisualCheckDone = request.VisualCheckDone;
            report.AdjustmentDone = request.AdjustmentDone;
            report.CleaningDone = request.CleaningDone;
            report.ShortDescription = request.ShortDescription;
            if (imageData is not null)
                report.ImageData = imageData;

            _reportRepository.Update(report);
        }

        if (request.Status.HasValue)
            order.Status = request.Status.Value;
        _orderRepository.Update(order);
        await _orderRepository.SaveChangesAsync();

        return ServiceResult<bool>.Ok(true);
    }

    public async Task<ServiceResult<bool>> DeleteAsync(int id)
    {
        var order = await _orderRepository.GetByIdAsync(id);
        if (order is null)
            return ServiceResult<bool>.NotFound($"Order {id} not found.");

        _orderRepository.Delete(order);
        await _orderRepository.SaveChangesAsync();

        return ServiceResult<bool>.Ok(true);
    }

    public async Task<ServiceResult<IEnumerable<MaintenanceOrderResponse>>> CreateDefaultPlanningAsync(int userId, int year, int month)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
            return ServiceResult<IEnumerable<MaintenanceOrderResponse>>.NotFound("User not found.");

        var elevators = await _elevatorRepository.GetAllByCompanyAsync(user.CompanyId);

        var alreadyScheduledIds = await _orderRepository.GetScheduledElevatorIdsAsync(user.CompanyId, year, month);
        var alreadyScheduledSet = alreadyScheduledIds.ToHashSet();

        var elevatorsToSchedule = elevators.Where(e => !alreadyScheduledSet.Contains(e.Id)).ToList();
        if (!elevatorsToSchedule.Any())
            return ServiceResult<IEnumerable<MaintenanceOrderResponse>>.Ok([]);

        var scheduledDate = new DateTime(year, month, 1);

        var orders = elevatorsToSchedule.Select(elevator => new MaintenanceOrder
        {
            ElevatorId = elevator.Id,
            MaintenanceType = MaintenanceType.Scheduled,
            ScheduledDate = scheduledDate,
            ShortDescription = "Default planing order",
            Status = OrderStatus.Open
        }).ToList();

        await _orderRepository.AddOrdersAsync(orders);
        await _orderRepository.SaveChangesAsync();

        var responses = orders.Select(o => new MaintenanceOrderResponse
        {
            Id = o.Id,
            ElevatorId = o.ElevatorId,
            MaintenanceType = o.MaintenanceType.ToString(),
            ScheduledDate = o.ScheduledDate,
            ShortDescription = o.ShortDescription,
            Status = o.Status
        });

        return ServiceResult<IEnumerable<MaintenanceOrderResponse>>.Ok(responses);
    }

    public async Task<ServiceResult<IEnumerable<MaintenanceOrderResponse>>> AutoPlanningAsync(int userId, int year, int month)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
            return ServiceResult<IEnumerable<MaintenanceOrderResponse>>.NotFound("User not found.");

        var unassignedOrders = (await _orderRepository.GetUnassignedScheduledOrdersAsync(year, month, user.CompanyId)).ToList();
        if (!unassignedOrders.Any())
            return ServiceResult<IEnumerable<MaintenanceOrderResponse>>.Ok([]);

        var prevMonth = month == 1 ? 12 : month - 1;
        var prevYear = month == 1 ? year - 1 : year;

        var elevatorIds = unassignedOrders.Select(o => o.ElevatorId).ToList();
        var prevMonthOrders = await _orderRepository.GetOrdersByElevatorIdsAndMonthAsync(elevatorIds, prevYear, prevMonth);
        var prevMonthMap = prevMonthOrders
            .GroupBy(o => o.ElevatorId)
            .ToDictionary(g => g.Key, g => g.First());

        foreach (var order in unassignedOrders)
        {
            if (!prevMonthMap.TryGetValue(order.ElevatorId, out var prev))
                continue;

            order.AssignedEngineerId = prev.AssignedEngineerId;
            order.ScheduledDate = new DateTime(year, month, prev.ScheduledDate.Day);
            _orderRepository.Update(order);
        }

        await _orderRepository.SaveChangesAsync();

        return ServiceResult<IEnumerable<MaintenanceOrderResponse>>.Ok(unassignedOrders.Select(MapToResponse));
    }

    private static MaintenanceOrderResponse MapToResponse(MaintenanceOrder o) => new()
    {
        Id = o.Id,
        ElevatorId = o.ElevatorId,
        ElevatorNumberInProject = o.Elevator?.NumberInProject ?? string.Empty,
        BuildingName = o.Elevator?.Building?.Name ?? string.Empty,
        BuildingAddress = o.Elevator?.Building?.Address ?? string.Empty,
        BuildingLatitude = o.Elevator?.Building?.Latitude,
        BuildingLongitude = o.Elevator?.Building?.Longitude,
        AssignedEngineerId = o.AssignedEngineerId,
        AssignedEngineerName = o.AssignedEngineer is null
            ? string.Empty
            : $"{o.AssignedEngineer.FirstName} {o.AssignedEngineer.LastName}",
        MaintenanceType = o.MaintenanceType.ToString(),
        ScheduledDate = o.ScheduledDate,
        Status = o.Status,
        ShortDescription = o.ShortDescription,
        Report = o.Report is null ? null : new MaintenanceReportResponse
        {
            JobStartedDate = o.Report.JobStartedDate,
            CompletedDate = o.Report.CompletedDate,
            IssueDetected = o.Report.IssueDetected,
            VisualCheckDone = o.Report.VisualCheckDone,
            AdjustmentDone = o.Report.AdjustmentDone,
            CleaningDone = o.Report.CleaningDone,
            ShortDescription = o.Report.ShortDescription,
            ImageBase64 = o.Report.ImageData is null ? null : Convert.ToBase64String(o.Report.ImageData)
        }
    };
}
