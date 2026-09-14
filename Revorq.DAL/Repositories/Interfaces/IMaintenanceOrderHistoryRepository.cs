using Revorq.DAL.Entities;

namespace Revorq.DAL.Repositories.Interfaces;

public interface IMaintenanceOrderHistoryRepository : IRepository<MaintenanceOrderHistory>
{
    new Task<MaintenanceOrderHistory?> GetByIdAsync(long id);
    Task<IEnumerable<MaintenanceOrderHistory>> GetByOrderIdsAsync(IEnumerable<long> orderIds);
}
