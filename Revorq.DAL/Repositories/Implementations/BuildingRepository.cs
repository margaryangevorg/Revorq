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
            .Include(b => b.Elevators)
            .Where(b => b.CompanyId == companyId)
            .AsQueryable();

        if (type is not null)
            query = query.Where(b => b.BuildingType == type);

        return await query.AsNoTracking().ToListAsync();
    }

    public async Task<Building?> GetWithElevatorsAsync(int buildingId, int companyId)
    {
        return await _context.Buildings
            .Include(b => b.Elevators)
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == buildingId && b.CompanyId == companyId);
    }

    public async Task<Building?> GetByNameAsync(string name, int companyId)
    {
        return await _context.Buildings
            .Include(b => b.Elevators)
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Name.ToLower() == name.ToLower() && b.CompanyId == companyId);
    }
}
