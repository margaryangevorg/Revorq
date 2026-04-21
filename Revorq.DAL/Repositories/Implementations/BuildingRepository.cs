using Revorq.DAL.Context;
using Revorq.DAL.Entities;
using Revorq.DAL.Enums;
using Revorq.DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Revorq.DAL.Repositories.Implementations;

public class BuildingRepository : Repository<Building>, IBuildingRepository
{
    public BuildingRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<Building>> GetAllAsync(int companyId, BuildingType? type)
    {
        var query = _context.Buildings
            .Include(b => b.Elevators.Where(e => e.Status == EntityStatus.Active))
            .Include(b => b.Files)
            .Where(b => b.CompanyId == companyId && b.Status == EntityStatus.Active)
            .AsQueryable();

        if (type is not null)
            query = query.Where(b => b.BuildingType == type);

        return await query.AsNoTracking().ToListAsync();
    }

    public async Task<Building?> GetWithElevatorsAsync(int buildingId, int companyId)
    {
        return await _context.Buildings
            .Include(b => b.Elevators.Where(e => e.Status == EntityStatus.Active))
            .Include(b => b.Files)
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == buildingId && b.CompanyId == companyId && b.Status == EntityStatus.Active);
    }

    public async Task<Building?> GetByNameAsync(string name, int companyId)
    {
        return await _context.Buildings
            .Include(b => b.Elevators.Where(e => e.Status == EntityStatus.Active))
            .Include(b => b.Files)
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Name.ToLower() == name.ToLower() && b.CompanyId == companyId && b.Status == EntityStatus.Active);
    }
}
