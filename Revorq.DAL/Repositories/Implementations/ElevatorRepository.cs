using Revorq.DAL.Context;
using Revorq.DAL.Entities;
using Revorq.DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Revorq.DAL.Repositories.Implementations;

public class ElevatorRepository : Repository<Elevator>, IElevatorRepository
{
    public ElevatorRepository(AppDbContext context) : base(context) { }

    public async Task<Elevator?> GetBySerialNumberAsync(string serialNumber)
    {
        return await _context.Elevators
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.SerialNumber == serialNumber);
    }

    public async Task<Elevator?> GetByNumberInProjectAsync(int buildingId, string numberInProject)
    {
        return await _context.Elevators
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.BuildingId == buildingId && e.NumberInProject == numberInProject);
    }

    public async Task<IEnumerable<Elevator>> GetByBuildingNameAsync(string buildingName)
    {
        return await _context.Elevators
            .Include(e => e.Building)
            .Where(e => e.Building.Name.ToLower() == buildingName.ToLower())
            .AsNoTracking()
            .ToListAsync();
    }
}
