using Revorq.DAL.Context;
using Revorq.DAL.Entities;
using Revorq.DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Revorq.DAL.Repositories.Implementations;

public class BuildingRepository : Repository<Building>, IBuildingRepository
{
    public BuildingRepository(AppDbContext context) : base(context) { }

    public async Task<Building?> GetWithElevatorsAsync(int buildingId)
    {
        return await _context.Buildings
            .Include(b => b.Elevators)
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == buildingId);
    }
}
