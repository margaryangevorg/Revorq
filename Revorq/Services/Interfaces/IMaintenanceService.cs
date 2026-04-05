using Revorq.API.Models;
using Revorq.API.Models.MaintenanceOrderModels;
using Revorq.DAL.Enums;

namespace Revorq.API.Services.Interfaces;

public interface IMaintenanceService
{
    Task<IEnumerable<MaintenanceOrderResponse>> GetOrdersUntilDateAsync(DateTime untilDate);
    Task<IEnumerable<MaintenanceOrderResponse>> GetMonthlyListAsync(int engineerId, int year, int month, OrderStatus? status);
    Task<IEnumerable<MaintenanceOrderResponse>> GetMonthlyAsync(int year, int month, OrderStatus? status, bool? isUnassigned);
    Task<IEnumerable<MaintenanceOrderResponse>> GetUnscheduledAsync();
    Task<ServiceResult<int>> CreateOrderAsync(CreateOrderRequest request);
    Task<ServiceResult<MaintenanceOrderResponse>> GetByIdAsync(int id);
    Task<ServiceResult<bool>> AssignOrderAsync(int orderId, int engineerId);
    Task<ServiceResult<bool>> CreateReportAsync(int orderId, CreateReportRequest request);
    Task<ServiceResult<bool>> DeleteAsync(int id);
}
