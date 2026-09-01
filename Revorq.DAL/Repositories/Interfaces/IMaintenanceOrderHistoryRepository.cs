using Revorq.DAL.Entities;

namespace Revorq.DAL.Repositories.Interfaces;

public interface IMaintenanceOrderHistoryRepository : IRepository<MaintenanceOrderHistory>
{
    Task<IEnumerable<MaintenanceOrderHistory>> GetByOrderIdsAsync(IEnumerable<int> orderIds);
}
