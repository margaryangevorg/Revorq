using Revorq.DAL.Entities;
using Revorq.DAL.Enums;

namespace Revorq.DAL.Repositories.Interfaces;

public interface IMaintenanceOrderRepository : IRepository<MaintenanceOrder>
{
    Task<IEnumerable<MaintenanceOrder>> GetMonthlyOrdersByEngineerAsync(int engineerId, int year, int month, OrderStatus? status);
    Task<IEnumerable<MaintenanceOrder>> GetOrdersUntilDateAsync(DateTime untilDate);
    Task<IEnumerable<MaintenanceOrder>> GetUnscheduledOrdersAsync();
}
