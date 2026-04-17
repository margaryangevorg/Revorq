using Revorq.DAL.Entities;
using Revorq.DAL.Enums;

namespace Revorq.DAL.Repositories.Interfaces;

public interface IMaintenanceOrderRepository : IRepository<MaintenanceOrder>
{
    Task<MaintenanceOrder?> GetByIdWithReportAsync(int id);
    Task AddOrdersAsync(IEnumerable<MaintenanceOrder> orders);
    Task<IEnumerable<int>> GetScheduledElevatorIdsAsync(int companyId, int year, int month);
    Task<IEnumerable<MaintenanceOrder>> GetUnassignedScheduledOrdersAsync(int year, int month, int companyId);
    Task<IEnumerable<MaintenanceOrder>> GetOrdersByElevatorIdsAndMonthAsync(IEnumerable<int> elevatorIds, int year, int month);
    Task<IEnumerable<MaintenanceOrder>> GetMonthlyOrdersAsync(int userId, int? assignedEngineerId, int year, int month, OrderStatus? status, bool? isUnassigned, bool? isScheduled);
    Task<IEnumerable<MaintenanceOrder>> GetOrdersUntilDateAsync(DateTime untilDate);
    Task<IEnumerable<MaintenanceOrder>> GetUnscheduledOrdersAsync();
}
