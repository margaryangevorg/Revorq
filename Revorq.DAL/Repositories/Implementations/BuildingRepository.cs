using Revorq.DAL.Context;
using Revorq.DAL.Entities;
using Revorq.DAL.Enums;
using Revorq.DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Revorq.DAL.Repositories.Implementations;

public class BuildingRepository : Repository<Building>, IBuildingRepository
{
    public BuildingRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<Building>> GetAllAsync(BuildingType? type)
    {
        var query = _context.Buildings
            .Include(b => b.Elevators)
            .AsQueryable();

        if (type is not null)
            query = query.Where(b => b.BuildingType == type);

        return await query.AsNoTracking().ToListAsync();
    }

    public async Task<Building?> GetWithElevatorsAsync(int buildingId)
    {
        return await _context.Buildings
            .Include(b => b.Elevators)
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == buildingId);
    }

    public async Task<Building?> GetByNameAsync(string name)
    {
        return await _context.Buildings
            .Include(b => b.Elevators)
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Name.ToLower() == name.ToLower());
    }
}
