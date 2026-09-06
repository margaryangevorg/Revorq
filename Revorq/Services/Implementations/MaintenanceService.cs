using ClosedXML.Excel;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Revorq.API.Models;
using Revorq.API.Models.MaintenanceOrderModels;
using Revorq.API.Services.Interfaces;
using Revorq.DAL.Entities;
using Revorq.DAL.Enums;
using Revorq.DAL.Repositories.Interfaces;
using Revorq.Models.MaintenanceOrderModels;

namespace Revorq.API.Services.Implementations;

public class MaintenanceService : IMaintenanceService
{
    private readonly IMaintenanceOrderRepository _orderRepository;
    private readonly IMaintenanceReportRepository _reportRepository;
    private readonly IMaintenanceOrderHistoryRepository _historyRepository;
    private readonly IElevatorRepository _elevatorRepository;
    private readonly IStorageService _storageService;
    private readonly UserManager<AppUser> _userManager;

    public MaintenanceService(
        IMaintenanceOrderRepository orderRepository,
        IMaintenanceReportRepository reportRepository,
        IMaintenanceOrderHistoryRepository historyRepository,
        IElevatorRepository elevatorRepository,
        IStorageService storageService,
        UserManager<AppUser> userManager)
    {
        _orderRepository = orderRepository;
        _reportRepository = reportRepository;
        _historyRepository = historyRepository;
        _elevatorRepository = elevatorRepository;
        _storageService = storageService;
        _userManager = userManager;
    }

    public async Task<IEnumerable<MaintenanceOrderResponse>> GetOrdersUntilDateAsync(DateTime untilDate)
    {
        var orders = await _orderRepository.GetOrdersUntilDateAsync(untilDate);
        return orders.Select(MapToResponse);
    }

    public async Task<IEnumerable<MaintenanceOrderResponse>> GetMonthlyAsync(int userId, MaintenanceMonthlyFilterModel filterModel)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
            return [];

        var roles = await _userManager.GetRolesAsync(user);
        int? assignedEngineerId = roles.Contains(nameof(Role.MaintenanceEngineer)) ? userId : null;

        var orders = await _orderRepository.GetMonthlyOrdersAsync(userId, assignedEngineerId, filterModel.Year, filterModel.Month, filterModel.Statuses, filterModel.IsUnassigned, filterModel.IsScheduled);
        return orders.Select(MapToResponse);
    }

    public async Task<IEnumerable<MaintenanceOrderResponse>> GetUnscheduledAsync()
    {
        var orders = await _orderRepository.GetUnscheduledOrdersAsync();
        return orders.Select(MapToResponse);
    }

    public async Task<ServiceResult<int>> CreateOrderAsync(OrderRequestInputModel request, int reporterId)
    {
        if (!request.ElevatorId.HasValue)
            return ServiceResult<int>.Error("ElevatorId is required.");

        var elevator = await _elevatorRepository.GetByIdAsync(request.ElevatorId.Value);
        if (elevator is null)
            return ServiceResult<int>.Error($"Elevator {request.ElevatorId} not found.");

        var order = new MaintenanceOrder
        {
            ElevatorId = request.ElevatorId.Value,
            AssignedEngineerId = request.AssignedEngineerId,
            MaintenanceType = request.MaintenanceType,
            ScheduledDate = DateTime.SpecifyKind(request.ScheduledDate.Date + DateTime.UtcNow.TimeOfDay, DateTimeKind.Utc),
            ShortDescription = request.ShortDescription,
            Status = OrderStatus.Open,
            ReporterId = reporterId
        };

        await _orderRepository.AddAsync(order);

        if (request.AssignedEngineerId.HasValue)
        {
            await _historyRepository.AddAsync(new MaintenanceOrderHistory
            {
                Order = order,
                Assignments = [new EngineerAssignment { EngineerId = request.AssignedEngineerId.Value, AssignedDate = DateTime.UtcNow }]
            });
        }

        await _orderRepository.SaveChangesAsync();

        if (request.Images.Count > 0)
        {
            var uploadTasks = request.Images.Select(img => _storageService.UploadMaintenanceOrderImageAsync(order.Id, img));
            order.ImageUrls.AddRange(await Task.WhenAll(uploadTasks));
            _orderRepository.Update(order);
            await _orderRepository.SaveChangesAsync();
        }

        return ServiceResult<int>.Ok(order.Id);
    }

    public async Task<ServiceResult<MaintenanceOrderResponse>> GetByIdAsync(int id)
    {
        var order = await _orderRepository.GetByIdWithReportAsync(id);
        if (order is null)
            return ServiceResult<MaintenanceOrderResponse>.NotFound($"Order {id} not found.");

        return ServiceResult<MaintenanceOrderResponse>.Ok(MapToResponse(order));
    }

    public async Task<ServiceResult<bool>> UpdateOrderAsync(int orderId, OrderRequestInputModel request, int userId)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);
        if (order is null)
            return ServiceResult<bool>.NotFound($"Order {orderId} not found.");

        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
            return ServiceResult<bool>.NotFound("User not found.");

        var roles = await _userManager.GetRolesAsync(user);
        var isAdminOrManager = roles.Contains(nameof(Role.Admin)) || roles.Contains(nameof(Role.Manager));

        if (order.ReporterId != userId && !isAdminOrManager)
            return ServiceResult<bool>.Error("You are not allowed to edit this order.");

        order.MaintenanceType = request.MaintenanceType;
        order.ScheduledDate = DateTime.SpecifyKind(request.ScheduledDate.Date + DateTime.UtcNow.TimeOfDay, DateTimeKind.Utc);
        order.ShortDescription = request.ShortDescription;

        if (request.AssignedEngineerId.HasValue && request.AssignedEngineerId != order.AssignedEngineerId)
        {
            order.AssignedEngineerId = request.AssignedEngineerId;

            var history = await _historyRepository.GetByIdAsync(orderId);
            if (history is null)
            {
                history = new MaintenanceOrderHistory { OrderId = orderId };
                await _historyRepository.AddAsync(history);
            }
            else
            {
                _historyRepository.Update(history);
            }
            history.Assignments.Add(new EngineerAssignment { EngineerId = request.AssignedEngineerId.Value, AssignedDate = DateTime.UtcNow });
        }

        _orderRepository.Update(order);

        if (request.Images.Count > 0)
        {
            var uploadTasks = request.Images.Select(img => _storageService.UploadMaintenanceOrderImageAsync(orderId, img));
            order.ImageUrls.AddRange(await Task.WhenAll(uploadTasks));
        }

        await _orderRepository.SaveChangesAsync();

        return ServiceResult<bool>.Ok(true);
    }

    public async Task<ServiceResult<bool>> AddOrderImagesAsync(int orderId, List<IFormFile> images, int userId)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);
        if (order is null)
            return ServiceResult<bool>.NotFound($"Order {orderId} not found.");

        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
            return ServiceResult<bool>.NotFound("User not found.");

        var roles = await _userManager.GetRolesAsync(user);
        var isAdminOrManager = roles.Contains(nameof(Role.Admin)) || roles.Contains(nameof(Role.Manager));

        if (order.ReporterId != userId && !isAdminOrManager)
            return ServiceResult<bool>.Error("You are not allowed to add images to this order.");

        var uploadTasks = images.Select(img => _storageService.UploadMaintenanceOrderImageAsync(orderId, img));
        order.ImageUrls.AddRange(await Task.WhenAll(uploadTasks));

        _orderRepository.Update(order);
        await _orderRepository.SaveChangesAsync();

        return ServiceResult<bool>.Ok(true);
    }

    public async Task<ServiceResult<bool>> DeleteOrderImagesAsync(int orderId, List<string> imageUrls, int userId)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);
        if (order is null)
            return ServiceResult<bool>.NotFound($"Order {orderId} not found.");

        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
            return ServiceResult<bool>.NotFound("User not found.");

        var roles = await _userManager.GetRolesAsync(user);
        var isAdminOrManager = roles.Contains(nameof(Role.Admin)) || roles.Contains(nameof(Role.Manager));

        if (order.ReporterId != userId && !isAdminOrManager)
            return ServiceResult<bool>.Error("You are not allowed to delete images from this order.");

        await Task.WhenAll(imageUrls.Select(url => _storageService.DeleteFileAsync(url)));
        order.ImageUrls.RemoveAll(url => imageUrls.Contains(url));

        _orderRepository.Update(order);
        await _orderRepository.SaveChangesAsync();

        return ServiceResult<bool>.Ok(true);
    }

    public async Task<ServiceResult<int>> CreateReportAsync(int orderId, CreateReportRequest request)
    {
        var order = await _orderRepository.GetByIdWithReportAsync(orderId);
        if (order is null)
            return ServiceResult<int>.NotFound($"Order {orderId} not found.");

        if (order.Status == OrderStatus.Done)
            return ServiceResult<int>.Error("Order is already completed.");

        var uploadedUrls = new List<string>();
        if (request.Images is { Count: > 0 })
        {
            var uploadTasks = request.Images.Select(img => _storageService.UploadMaintenanceReportImageAsync(orderId, img));
            uploadedUrls.AddRange(await Task.WhenAll(uploadTasks));
        }

        MaintenanceReport report;
        if (order.Report is null)
        {
            report = new MaintenanceReport
            {
                OrderId = orderId,
                JobStartedDate = AsUtc(request.JobStartedDate),
                CompletedDate = AsUtc(request.CompletedDate),
                IssueDetected = request.IssueDetected,
                VisualCheckDone = request.VisualCheckDone,
                AdjustmentDone = request.AdjustmentDone,
                CleaningDone = request.CleaningDone,
                IsPartChange = request.IsPartChange,
                Notes = request.Notes,
                ImageUrls = uploadedUrls
            };

            await _reportRepository.AddAsync(report);
        }
        else
        {
            report = order.Report;
            report.JobStartedDate = AsUtc(request.JobStartedDate);
            report.CompletedDate = AsUtc(request.CompletedDate);
            report.IssueDetected = request.IssueDetected;
            report.VisualCheckDone = request.VisualCheckDone;
            report.AdjustmentDone = request.AdjustmentDone;
            report.CleaningDone = request.CleaningDone;
            report.IsPartChange = request.IsPartChange;
            report.Notes = request.Notes;
            if (uploadedUrls.Count > 0)
                report.ImageUrls.AddRange(uploadedUrls);

            _reportRepository.Update(report);
        }

        if (request.Status.HasValue)
            order.Status = request.Status.Value;
        _orderRepository.Update(order);
        await _orderRepository.SaveChangesAsync();

        return ServiceResult<int>.Ok(report.OrderId);
    }

    public async Task<ServiceResult<bool>> UpdateReportAsync(int orderId, UpdateReportRequest request, int userId)
    {
        var order = await _orderRepository.GetByIdWithReportAsync(orderId);
        if (order is null)
            return ServiceResult<bool>.NotFound($"Order {orderId} not found.");

        if (order.Report is null)
            return ServiceResult<bool>.NotFound($"Order {orderId} has no report.");

        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
            return ServiceResult<bool>.NotFound("User not found.");

        var roles = await _userManager.GetRolesAsync(user);
        if (!roles.Contains(nameof(Role.Admin)) && !roles.Contains(nameof(Role.Manager)))
            return ServiceResult<bool>.Error("You are not allowed to edit this report.");

        var report = order.Report;
        report.JobStartedDate = AsUtc(request.JobStartedDate);
        report.CompletedDate = AsUtc(request.CompletedDate);
        report.IssueDetected = request.IssueDetected;
        report.VisualCheckDone = request.VisualCheckDone;
        report.AdjustmentDone = request.AdjustmentDone;
        report.CleaningDone = request.CleaningDone;
        report.IsPartChange = request.IsPartChange;
        report.Notes = request.Notes;

        if (request.Status.HasValue)
            order.Status = request.Status.Value;

        _reportRepository.Update(report);
        _orderRepository.Update(order);
        await _orderRepository.SaveChangesAsync();

        return ServiceResult<bool>.Ok(true);
    }

    public async Task<ServiceResult<bool>> AddReportImagesAsync(int orderId, List<IFormFile> images, int userId)
    {
        var order = await _orderRepository.GetByIdWithReportAsync(orderId);
        if (order is null)
            return ServiceResult<bool>.NotFound($"Order {orderId} not found.");

        if (order.Report is null)
            return ServiceResult<bool>.NotFound($"Order {orderId} has no report.");

        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
            return ServiceResult<bool>.NotFound("User not found.");

        var roles = await _userManager.GetRolesAsync(user);
        if (!roles.Contains(nameof(Role.Admin)) && !roles.Contains(nameof(Role.Manager)))
            return ServiceResult<bool>.Error("You are not allowed to add images to this report.");

        var uploadTasks = images.Select(img => _storageService.UploadMaintenanceReportImageAsync(orderId, img));
        order.Report.ImageUrls.AddRange(await Task.WhenAll(uploadTasks));

        _reportRepository.Update(order.Report);
        await _reportRepository.SaveChangesAsync();

        return ServiceResult<bool>.Ok(true);
    }

    public async Task<ServiceResult<bool>> DeleteReportImagesAsync(int orderId, List<string> imageUrls, int userId)
    {
        var order = await _orderRepository.GetByIdWithReportAsync(orderId);
        if (order is null)
            return ServiceResult<bool>.NotFound($"Order {orderId} not found.");

        if (order.Report is null)
            return ServiceResult<bool>.NotFound($"Order {orderId} has no report.");

        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
            return ServiceResult<bool>.NotFound("User not found.");

        var roles = await _userManager.GetRolesAsync(user);
        if (!roles.Contains(nameof(Role.Admin)) && !roles.Contains(nameof(Role.Manager)))
            return ServiceResult<bool>.Error("You are not allowed to delete images from this report.");

        await Task.WhenAll(imageUrls.Select(url => _storageService.DeleteFileAsync(url)));
        order.Report.ImageUrls.RemoveAll(url => imageUrls.Contains(url));

        _reportRepository.Update(order.Report);
        await _reportRepository.SaveChangesAsync();

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

        var scheduledDate = DateTime.SpecifyKind(new DateTime(year, month, 1) + DateTime.UtcNow.TimeOfDay, DateTimeKind.Utc);

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

        var elevators = (await _elevatorRepository.GetAllByCompanyAsync(user.CompanyId)).ToList();
        if (!elevators.Any())
            return ServiceResult<IEnumerable<MaintenanceOrderResponse>>.Ok([]);

        var elevatorIds = elevators.Select(e => e.Id).ToList();

        var existingOrders = await _orderRepository.GetOrdersByElevatorIdsAndMonthAsync(elevatorIds, year, month);
        var existingCountByElevator = existingOrders
            .GroupBy(o => o.ElevatorId)
            .ToDictionary(g => g.Key, g => g.Count());

        var scheduledDate = DateTime.SpecifyKind(new DateTime(year, month, 1) + DateTime.UtcNow.TimeOfDay, DateTimeKind.Utc);
        var newOrders = new List<MaintenanceOrder>();

        foreach (var elevator in elevators)
        {
            var existingCount = existingCountByElevator.GetValueOrDefault(elevator.Id, 0);
            var toCreate = elevator.MonthlyDefaultOrdersCount - existingCount;

            for (var i = 0; i < toCreate; i++)
            {
                newOrders.Add(new MaintenanceOrder
                {
                    ElevatorId = elevator.Id,
                    MaintenanceType = MaintenanceType.Scheduled,
                    ScheduledDate = scheduledDate,
                    ShortDescription = "Default planning order",
                    Status = OrderStatus.Open,
                    ReporterId = userId
                });
            }
        }

        if (newOrders.Any())
            await _orderRepository.AddOrdersAsync(newOrders);

        var prevMonth = month == 1 ? 12 : month - 1;
        var prevYear = month == 1 ? year - 1 : year;

        var prevMonthOrders = await _orderRepository.GetOrdersByElevatorIdsAndMonthAsync(elevatorIds, prevYear, prevMonth);
        var prevMonthByElevator = prevMonthOrders
            .GroupBy(o => o.ElevatorId)
            .ToDictionary(g => g.Key, g => g.ToList());

        var newOrdersByElevator = newOrders
            .GroupBy(o => o.ElevatorId)
            .ToDictionary(g => g.Key, g => g.ToList());

        foreach (var (elevatorId, elevatorNewOrders) in newOrdersByElevator)
        {
            if (!prevMonthByElevator.TryGetValue(elevatorId, out var prevOrders))
                continue;

            for (var i = 0; i < elevatorNewOrders.Count && i < prevOrders.Count; i++)
            {
                var previousEngineerId = prevOrders[i].AssignedEngineerId;
                elevatorNewOrders[i].AssignedEngineerId = previousEngineerId;
                elevatorNewOrders[i].ScheduledDate = DateTime.SpecifyKind(new DateTime(year, month, prevOrders[i].ScheduledDate.Day) + DateTime.UtcNow.TimeOfDay, DateTimeKind.Utc);

                if (previousEngineerId.HasValue)
                {
                    await _historyRepository.AddAsync(new MaintenanceOrderHistory
                    {
                        Order = elevatorNewOrders[i],
                        Assignments = [new EngineerAssignment { EngineerId = previousEngineerId.Value, AssignedDate = DateTime.UtcNow }]
                    });
                }
            }
        }

        if (newOrders.Any())
            await _orderRepository.SaveChangesAsync();

        return ServiceResult<IEnumerable<MaintenanceOrderResponse>>.Ok(newOrders.Select(MapToResponse));
    }

    public async Task<byte[]> ExportMonthlyReportsAsync(int userId, int year, int month)
    {
        var filterModel = new MaintenanceMonthlyFilterModel
        {
            Year = year,
            Month = month,
            Statuses = Enum.GetValues<OrderStatus>().ToList()
        };

        var orders = (await GetMonthlyAsync(userId, filterModel)).ToList();

        var histories = (await _historyRepository.GetByOrderIdsAsync(orders.Select(o => o.Id)))
            .ToDictionary(h => h.OrderId);

        var engineerIds = histories.Values
            .SelectMany(h => h.Assignments)
            .Select(a => a.EngineerId)
            .Distinct()
            .ToList();

        var engineerNames = engineerIds.Count == 0
            ? new Dictionary<int, string>()
            : await _userManager.Users
                .Where(u => engineerIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => $"{u.FirstName} {u.LastName}");

        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add($"{year}-{month:D2}");

        string[] headers =
        [
            "Order #", "Elevator", "Building", "Address", "Assigned Engineer", "Assignment History",
            "Type", "Scheduled Date", "Status", "Order Description",
            "Job Started", "Completed", "Issue Detected", "Visual Check",
            "Adjustment", "Cleaning", "Part Change", "Report Description"
        ];

        for (var i = 0; i < headers.Length; i++)
        {
            var cell = ws.Cell(1, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
        }

        var row = 2;
        foreach (var o in orders)
        {
            ws.Cell(row, 1).Value = o.Id;
            ws.Cell(row, 2).Value = o.ElevatorNumberInProject;
            ws.Cell(row, 3).Value = o.BuildingName;
            ws.Cell(row, 4).Value = o.BuildingAddress;
            ws.Cell(row, 5).Value = o.AssignedEngineerName ?? string.Empty;
            ws.Cell(row, 6).Value = histories.TryGetValue(o.Id, out var history)
                ? string.Join("; ", history.Assignments
                    .OrderBy(a => a.AssignedDate)
                    .Select(a => $"{(engineerNames.TryGetValue(a.EngineerId, out var name) ? name : string.Empty)} ({a.AssignedDate:yyyy-MM-dd HH:mm})"))
                : string.Empty;
            ws.Cell(row, 7).Value = o.MaintenanceType;
            ws.Cell(row, 8).Value = o.ScheduledDate.ToString("yyyy-MM-dd");
            ws.Cell(row, 9).Value = o.Status.ToString();
            ws.Cell(row, 10).Value = o.ShortDescription ?? string.Empty;
            ws.Cell(row, 11).Value = o.Report?.JobStartedDate?.ToString("yyyy-MM-dd") ?? string.Empty;
            ws.Cell(row, 12).Value = o.Report?.CompletedDate?.ToString("yyyy-MM-dd") ?? string.Empty;
            ws.Cell(row, 13).Value = o.Report?.IssueDetected == true ? "Yes" : "No";
            ws.Cell(row, 14).Value = o.Report?.VisualCheckDone == true ? "Yes" : "No";
            ws.Cell(row, 15).Value = o.Report?.AdjustmentDone == true ? "Yes" : "No";
            ws.Cell(row, 16).Value = o.Report?.CleaningDone == true ? "Yes" : "No";
            ws.Cell(row, 17).Value = o.Report?.IsPartChange == true ? "Yes" : "No";
            ws.Cell(row, 18).Value = o.Report?.Notes ?? string.Empty;
            row++;
        }

        ws.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    private static DateTime? AsUtc(DateTime? value) =>
        value.HasValue ? DateTime.SpecifyKind(value.Value, DateTimeKind.Utc) : null;

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
        ReporterId = o.ReporterId,
        ReporterName = o.Reporter is null
            ? string.Empty
            : $"{o.Reporter.FirstName} {o.Reporter.LastName}",
        MaintenanceType = o.MaintenanceType.ToString(),
        ScheduledDate = o.ScheduledDate,
        Status = o.Status,
        ShortDescription = o.ShortDescription,
        ImageUrls = o.ImageUrls,
        CreatedDate = o.CreatedDate,
        UpdatedDate = o.UpdatedDate,
        Report = o.Report is null ? null : new MaintenanceReportResponse
        {
            JobStartedDate = o.Report.JobStartedDate,
            CompletedDate = o.Report.CompletedDate,
            IssueDetected = o.Report.IssueDetected,
            VisualCheckDone = o.Report.VisualCheckDone,
            AdjustmentDone = o.Report.AdjustmentDone,
            CleaningDone = o.Report.CleaningDone,
            IsPartChange = o.Report.IsPartChange,
            Notes = o.Report.Notes,
            ImageUrls = o.Report.ImageUrls,
            CreatedDate = o.Report.CreatedDate,
            UpdatedDate = o.Report.UpdatedDate
        }
    };
}
