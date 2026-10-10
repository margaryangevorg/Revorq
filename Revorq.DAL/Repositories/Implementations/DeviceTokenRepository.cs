using Microsoft.EntityFrameworkCore;
using Revorq.DAL.Context;
using Revorq.DAL.Entities;
using Revorq.DAL.Repositories.Interfaces;

namespace Revorq.DAL.Repositories.Implementations;

public class DeviceTokenRepository : Repository<DeviceToken>, IDeviceTokenRepository
{
    public DeviceTokenRepository(AppDbContext context) : base(context) { }

    public async Task<DeviceToken?> GetByTokenAsync(string token)
    {
        return await _context.DeviceTokens.FirstOrDefaultAsync(t => t.Token == token);
    }

    public async Task<IEnumerable<DeviceToken>> GetByUserIdsAsync(IEnumerable<int> userIds)
    {
        return await _context.DeviceTokens
            .Where(t => userIds.Contains(t.UserId))
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task DeleteByTokenAsync(string token)
    {
        var existing = await _context.DeviceTokens.FirstOrDefaultAsync(t => t.Token == token);
        if (existing is not null)
            _context.DeviceTokens.Remove(existing);
    }
}
