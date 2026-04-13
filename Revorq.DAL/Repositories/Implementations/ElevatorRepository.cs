using Revorq.DAL.Context;
using Revorq.DAL.Entities;
using Revorq.DAL.Enums;
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
            .FirstOrDefaultAsync(e => e.SerialNumber == serialNumber && e.Status == EntityStatus.Active);
    }

    public async Task<Elevator?> GetByNumberInProjectAsync(int buildingId, string numberInProject)
    {
        return await _context.Elevators
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.BuildingId == buildingId && e.NumberInProject == numberInProject && e.Status == EntityStatus.Active);
    }

    public async Task<Elevator?> GetWithBuildingAsync(int id)
    {
        return await _context.Elevators
            .Include(e => e.Building)
            .FirstOrDefaultAsync(e => e.Id == id && e.Status == EntityStatus.Active);
    }

    public async Task<IEnumerable<Elevator>> GetAllByUserAsync(int userId)
    {
        return await _context.UserBuildingAccesses
            .Where(a => a.UserId == userId)
            .Include(a => a.Building)
                .ThenInclude(b => b.Elevators)
            .SelectMany(a => a.Building.Elevators.Where(e => e.Status == EntityStatus.Active).Select(e => new Elevator
            {
                Id = e.Id,
                NumberInProject = e.NumberInProject,
                SerialNumber = e.SerialNumber,
                Model = e.Model,
                ProductionCountry = e.ProductionCountry,
                CustomerFullName = e.CustomerFullName,
                CustomerPhoneNumber = e.CustomerPhoneNumber,
                WarrantyType = e.WarrantyType,
                WarrantyDate = e.WarrantyDate,
                Priority = e.Priority,
                CreationDate = e.CreationDate,
                BuildingId = e.BuildingId,
                Building = a.Building
            }))
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task UpdateStatusByBuildingAsync(int buildingId, EntityStatus status)
    {
        await _context.Elevators
            .Where(e => e.BuildingId == buildingId)
            .ExecuteUpdateAsync(s => s.SetProperty(e => e.Status, status));
    }

    public async Task<IEnumerable<Elevator>> GetAllByCompanyAsync(int companyId)
    {
        return await _context.Elevators
            .Where(e => e.Building.CompanyId == companyId && e.Status == EntityStatus.Active)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<Elevator>> GetByBuildingNameByUserAsync(string buildingName, int userId)
    {
        return await _context.UserBuildingAccesses
            .Where(a => a.UserId == userId && a.Building.Name.ToLower() == buildingName.ToLower())
            .Include(a => a.Building)
                .ThenInclude(b => b.Elevators)
            .SelectMany(a => a.Building.Elevators.Where(e => e.Status == EntityStatus.Active).Select(e => new Elevator
            {
                Id = e.Id,
                NumberInProject = e.NumberInProject,
                SerialNumber = e.SerialNumber,
                Model = e.Model,
                ProductionCountry = e.ProductionCountry,
                CustomerFullName = e.CustomerFullName,
                CustomerPhoneNumber = e.CustomerPhoneNumber,
                WarrantyType = e.WarrantyType,
                WarrantyDate = e.WarrantyDate,
                Priority = e.Priority,
                CreationDate = e.CreationDate,
                BuildingId = e.BuildingId,
                Building = a.Building
            }))
            .AsNoTracking()
            .ToListAsync();
    }
}
