using Revorq.DAL.Entities;

namespace Revorq.DAL.Repositories.Interfaces;

public interface IMaintenanceOrderRepository : IRepository<MaintenanceOrder>
{
    Task<IEnumerable<MaintenanceOrder>> GetMonthlyOrdersByEngineerAsync(int engineerId, int year, int month);
    Task<IEnumerable<MaintenanceOrder>> GetOrdersUntilDateAsync(DateTime untilDate);
    Task<IEnumerable<MaintenanceOrder>> GetUnscheduledOrdersAsync();
}
