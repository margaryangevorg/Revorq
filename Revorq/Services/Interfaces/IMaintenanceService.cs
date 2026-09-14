using Revorq.API.Models;
using Revorq.API.Models.MaintenanceOrderModels;
using Revorq.DAL.Enums;
using Revorq.Models.MaintenanceOrderModels;

namespace Revorq.API.Services.Interfaces;

public interface IMaintenanceService
{
    Task<IEnumerable<MaintenanceOrderResponse>> GetOrdersUntilDateAsync(DateTime untilDate);
    Task<IEnumerable<MaintenanceOrderResponse>> GetMonthlyAsync(int userId, MaintenanceMonthlyFilterModel filterModel);
    Task<IEnumerable<MaintenanceOrderResponse>> GetUnscheduledAsync();
    Task<ServiceResult<long>> CreateOrderAsync(OrderRequestInputModel request, int reporterId);
    Task<ServiceResult<MaintenanceOrderResponse>> GetByIdAsync(long id);
    Task<ServiceResult<bool>> UpdateOrderAsync(long orderId, OrderRequestInputModel request, int userId);
    Task<ServiceResult<bool>> AddOrderImagesAsync(long orderId, List<IFormFile> images, int userId);
    Task<ServiceResult<bool>> DeleteOrderImagesAsync(long orderId, List<string> imageUrls, int userId);
    Task<ServiceResult<long>> CreateReportAsync(long orderId, CreateReportRequest request);
    Task<ServiceResult<bool>> UpdateReportAsync(long orderId, UpdateReportRequest request, int userId);
    Task<ServiceResult<bool>> AddReportImagesAsync(long orderId, List<IFormFile> images, int userId);
    Task<ServiceResult<bool>> DeleteReportImagesAsync(long orderId, List<string> imageUrls, int userId);
    Task<ServiceResult<bool>> DeleteAsync(long id);
    Task<ServiceResult<IEnumerable<MaintenanceOrderResponse>>> CreateDefaultPlanningAsync(int userId, int year, int month);
    Task<ServiceResult<IEnumerable<MaintenanceOrderResponse>>> AutoPlanningAsync(int userId, int year, int month);
    Task<byte[]> ExportMonthlyReportsAsync(int userId, int year, int month);
}
