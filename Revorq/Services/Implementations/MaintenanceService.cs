using Revorq.API.Models;
using Revorq.API.Models.MaintenanceOrderModels;
using Revorq.API.Services.Interfaces;
using Revorq.DAL.Entities;
using Revorq.DAL.Enums;
using Revorq.DAL.Repositories.Interfaces;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace Revorq.API.Services.Implementations;

public class MaintenanceService : IMaintenanceService
{
    private readonly IMaintenanceOrderRepository _orderRepository;
    private readonly IMaintenanceReportRepository _reportRepository;
    private readonly IRepository<Elevator> _elevatorRepository;
    private readonly IWebHostEnvironment _env;

    public MaintenanceService(
        IMaintenanceOrderRepository orderRepository,
        IMaintenanceReportRepository reportRepository,
        IRepository<Elevator> elevatorRepository,
        IWebHostEnvironment env)
    {
        _orderRepository = orderRepository;
        _reportRepository = reportRepository;
        _elevatorRepository = elevatorRepository;
        _env = env;
    }

    public async Task<IEnumerable<MaintenanceOrderResponse>> GetOrdersUntilDateAsync(DateTime untilDate)
    {
        var orders = await _orderRepository.GetOrdersUntilDateAsync(untilDate);
        return orders.Select(MapToResponse);
    }

    public async Task<IEnumerable<MaintenanceOrderResponse>> GetMonthlyListAsync(int engineerId, int year, int month, OrderStatus? status)
    {
        var orders = await _orderRepository.GetMonthlyOrdersByEngineerAsync(engineerId, year, month, status);
        return orders.Select(MapToResponse);
    }

    public async Task<IEnumerable<MaintenanceOrderResponse>> GetMonthlyAsync(int year, int month, OrderStatus? status, bool? isUnassigned)
    {
        var orders = await _orderRepository.GetMonthlyOrdersAsync(year, month, status, isUnassigned);
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
        var order = await _orderRepository.GetByIdAsync(id);
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
        var order = await _orderRepository.GetByIdAsync(orderId);
        if (order is null)
            return ServiceResult<bool>.NotFound($"Order {orderId} not found.");

        if (order.Status == OrderStatus.Done)
            return ServiceResult<bool>.Error("A report already exists for this order.");

        string? imagePath = null;
        if (request.Image is not null)
        {
            var folder = Path.Combine(_env.ContentRootPath, "uploads", "orders");
            Directory.CreateDirectory(folder);
            var fileName = $"{Guid.NewGuid()}.jpg";
            var filePath = Path.Combine(folder, fileName);

            using var image = await Image.LoadAsync(request.Image.OpenReadStream());
            image.Mutate(x => x.Resize(new ResizeOptions
            {
                Size = new Size(1024, 1024),
                Mode = ResizeMode.Max
            }));
            await image.SaveAsJpegAsync(filePath);
            imagePath = $"/uploads/orders/{fileName}";
        }

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
            ImagePath = imagePath
        };

        order.Status = OrderStatus.Done;

        await _reportRepository.AddAsync(report);
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

    private static MaintenanceOrderResponse MapToResponse(MaintenanceOrder o) => new()
    {
        Id = o.Id,
        ElevatorId = o.ElevatorId,
        ElevatorNumberInProject = o.Elevator?.NumberInProject ?? string.Empty,
        BuildingName = o.Elevator?.Building?.Name ?? string.Empty,
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
            ImagePath = o.Report.ImagePath
        }
    };
}
