using Revorq.DAL.Entities;
using Revorq.DAL.Enums;

namespace Revorq.DAL.Repositories.Interfaces;

public interface IMaintenanceOrderRepository : IRepository<MaintenanceOrder>
{
    Task<MaintenanceOrder?> GetByIdWithReportAsync(int id);
    Task<IEnumerable<MaintenanceOrder>> GetMonthlyOrdersAsync(int userId, int? engineerId, int year, int month, OrderStatus? status, bool? isUnassigned);
    Task<IEnumerable<MaintenanceOrder>> GetOrdersUntilDateAsync(DateTime untilDate);
    Task<IEnumerable<MaintenanceOrder>> GetUnscheduledOrdersAsync();
}
