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

    public async Task<Elevator?> GetWithBuildingAsync(int id)
    {
        return await _context.Elevators
            .Include(e => e.Building)
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<IEnumerable<Elevator>> GetAllByCompanyAsync(int companyId)
    {
        return await _context.Elevators
            .Include(e => e.Building)
            .Where(e => e.Building.CompanyId == companyId)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<Elevator>> GetByBuildingNameAsync(string buildingName, int companyId)
    {
        return await _context.Elevators
            .Include(e => e.Building)
            .Where(e => e.Building.Name.ToLower() == buildingName.ToLower() && e.Building.CompanyId == companyId)
            .AsNoTracking()
            .ToListAsync();
    }
}
