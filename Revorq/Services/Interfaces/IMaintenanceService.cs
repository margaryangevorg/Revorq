using Revorq.API.Models;
using Revorq.API.Models.MaintenanceOrderModels;
using Revorq.DAL.Enums;

namespace Revorq.API.Services.Interfaces;

public interface IMaintenanceService
{
    Task<IEnumerable<MaintenanceOrderResponse>> GetOrdersUntilDateAsync(DateTime untilDate);
    Task<IEnumerable<MaintenanceOrderResponse>> GetMonthlyAsync(int userId, int year, int month, OrderStatus? status, bool? isUnassigned, bool? isScheduled);
    Task<IEnumerable<MaintenanceOrderResponse>> GetUnscheduledAsync();
    Task<ServiceResult<int>> CreateOrderAsync(CreateOrderRequest request);
    Task<ServiceResult<MaintenanceOrderResponse>> GetByIdAsync(int id);
    Task<ServiceResult<bool>> AssignOrderAsync(int orderId, int engineerId);
    Task<ServiceResult<bool>> UpdateOrderAsync(int orderId, UpdateOrderRequest request, int userId);
    Task<ServiceResult<bool>> AddOrderImagesAsync(int orderId, List<IFormFile> images, int userId);
    Task<ServiceResult<bool>> DeleteOrderImagesAsync(int orderId, List<string> imageUrls, int userId);
    Task<ServiceResult<bool>> CreateReportAsync(int orderId, CreateReportRequest request);
    Task<ServiceResult<bool>> UpdateReportAsync(int orderId, UpdateReportRequest request, int userId);
    Task<ServiceResult<bool>> AddReportImagesAsync(int orderId, List<IFormFile> images, int userId);
    Task<ServiceResult<bool>> DeleteReportImagesAsync(int orderId, List<string> imageUrls, int userId);
    Task<ServiceResult<bool>> DeleteAsync(int id);
    Task<ServiceResult<IEnumerable<MaintenanceOrderResponse>>> CreateDefaultPlanningAsync(int userId, int year, int month);
    Task<ServiceResult<IEnumerable<MaintenanceOrderResponse>>> AutoPlanningAsync(int userId, int year, int month);
}
