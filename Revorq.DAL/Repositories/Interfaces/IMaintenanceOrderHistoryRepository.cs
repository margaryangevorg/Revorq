using Revorq.DAL.Entities;

namespace Revorq.DAL.Repositories.Interfaces;

public interface IMaintenanceOrderHistoryRepository : IRepository<MaintenanceOrderHistory>
{
    Task<MaintenanceOrderHistory?> GetByIdAsync(long id);
    Task<IEnumerable<MaintenanceOrderHistory>> GetByOrderIdsAsync(IEnumerable<long> orderIds);
}
