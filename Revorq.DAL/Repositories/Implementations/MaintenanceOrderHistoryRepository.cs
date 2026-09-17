using Microsoft.EntityFrameworkCore;
using Revorq.DAL.Context;
using Revorq.DAL.Entities;
using Revorq.DAL.Repositories.Interfaces;

namespace Revorq.DAL.Repositories.Implementations;

public class MaintenanceOrderHistoryRepository : Repository<MaintenanceOrderHistory>, IMaintenanceOrderHistoryRepository
{
    public MaintenanceOrderHistoryRepository(AppDbContext context) : base(context) { }

    public async Task<MaintenanceOrderHistory?> GetByIdAsync(long id) =>
        await _dbSet.FindAsync(id);

    public async Task<IEnumerable<MaintenanceOrderHistory>> GetByOrderIdsAsync(IEnumerable<long> orderIds)
    {
        return await _context.MaintenanceOrderHistories
            .Where(h => orderIds.Contains(h.OrderId))
            .AsNoTracking()
            .ToListAsync();
    }
}
