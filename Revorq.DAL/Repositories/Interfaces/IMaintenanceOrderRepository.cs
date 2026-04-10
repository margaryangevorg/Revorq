using Revorq.DAL.Entities;
using Revorq.DAL.Enums;

namespace Revorq.DAL.Repositories.Interfaces;

public interface IMaintenanceOrderRepository : IRepository<MaintenanceOrder>
{
    Task<MaintenanceOrder?> GetByIdWithReportAsync(int id);
    Task AddOrdersAsync(IEnumerable<MaintenanceOrder> orders);
    Task<IEnumerable<int>> GetScheduledElevatorIdsAsync(IEnumerable<int> elevatorIds, int year, int month);
    Task<IEnumerable<MaintenanceOrder>> GetMonthlyOrdersAsync(int? engineerId, int year, int month, OrderStatus? status, bool? isUnassigned, bool? isScheduled);
    Task<IEnumerable<MaintenanceOrder>> GetOrdersUntilDateAsync(DateTime untilDate);
    Task<IEnumerable<MaintenanceOrder>> GetUnscheduledOrdersAsync();
}
