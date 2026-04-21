using Revorq.DAL.Context;
using Revorq.DAL.Entities;
using Revorq.DAL.Enums;
using Revorq.DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Revorq.DAL.Repositories.Implementations;

public class UserBuildingAccessRepository : IUserBuildingAccessRepository
{
    private readonly AppDbContext _context;

    public UserBuildingAccessRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<AppUser>> GetUsersWithAccessAsync(int buildingId)
    {
        return await _context.UserBuildingAccesses
            .Where(a => a.BuildingId == buildingId)
            .Select(a => a.User)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<Building>> GetBuildingsForUserAsync(int userId)
    {
        return await _context.UserBuildingAccesses
            .Where(a => a.UserId == userId && a.Building.Status == EntityStatus.Active)
            .Include(a => a.Building)
                .ThenInclude(b => b.Elevators.Where(e => e.Status == EntityStatus.Active))
            .Include(a => a.Building)
                .ThenInclude(b => b.Files)
            .Select(a => a.Building)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<bool> ExistsAsync(int userId, int buildingId)
    {
        return await _context.UserBuildingAccesses
            .AnyAsync(a => a.UserId == userId && a.BuildingId == buildingId);
    }

    public async Task GrantAsync(int userId, int buildingId)
    {
        _context.UserBuildingAccesses.Add(new UserBuildingAccess
        {
            UserId = userId,
            BuildingId = buildingId
        });
        await Task.CompletedTask;
    }

    public async Task RevokeAsync(int userId, int buildingId)
    {
        var access = await _context.UserBuildingAccesses
            .FirstOrDefaultAsync(a => a.UserId == userId && a.BuildingId == buildingId);

        if (access is not null)
            _context.UserBuildingAccesses.Remove(access);
    }

    public async Task SaveChangesAsync() => await _context.SaveChangesAsync();
}
