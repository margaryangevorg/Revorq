using Revorq.DAL.Context;
using Revorq.DAL.Entities;
using Revorq.DAL.Enums;
using Revorq.DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Revorq.DAL.Repositories.Implementations;

public class MaintenanceOrderRepository : Repository<MaintenanceOrder>, IMaintenanceOrderRepository
{
    public MaintenanceOrderRepository(AppDbContext context) : base(context) { }

    public async Task AddOrdersAsync(IEnumerable<MaintenanceOrder> orders)
    {
        await _context.MaintenanceOrders.AddRangeAsync(orders);
    }

    public async Task<IEnumerable<int>> GetScheduledElevatorIdsAsync(int companyId, int year, int month)
    {
        return await _context.MaintenanceOrders
            .Where(o => o.Elevator.Building.CompanyId == companyId
                     && o.MaintenanceType == MaintenanceType.Scheduled
                     && o.ScheduledDate.Year == year
                     && o.ScheduledDate.Month == month)
            .Select(o => o.ElevatorId)
            .Distinct()
            .ToListAsync();
    }

    public async Task<IEnumerable<MaintenanceOrder>> GetUnassignedScheduledOrdersAsync(int year, int month, int companyId)
    {
        return await _context.MaintenanceOrders
            .Where(o => o.MaintenanceType == MaintenanceType.Scheduled
                     && o.AssignedEngineerId == null
                     && o.ScheduledDate.Year == year
                     && o.ScheduledDate.Month == month
                     && o.Elevator.Building.CompanyId == companyId)
            .ToListAsync();
    }

    public async Task<IEnumerable<MaintenanceOrder>> GetOrdersByElevatorIdsAndMonthAsync(IEnumerable<int> elevatorIds, int year, int month)
    {
        return await _context.MaintenanceOrders
            .Where(o => elevatorIds.Contains(o.ElevatorId)
                     && o.MaintenanceType == MaintenanceType.Scheduled
                     && o.ScheduledDate.Year == year
                     && o.ScheduledDate.Month == month
                     && o.AssignedEngineerId != null)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<MaintenanceOrder?> GetByIdWithReportAsync(int id)
    {
        return await _context.MaintenanceOrders
            .Include(o => o.Elevator)
                .ThenInclude(el => el.Building)
            .Include(o => o.AssignedEngineer)
            .Include(o => o.Report)
            .FirstOrDefaultAsync(o => o.Id == id);
    }

    public async Task<IEnumerable<MaintenanceOrder>> GetMonthlyOrdersAsync(
        int? companyId, int? engineerId, int year, int month, OrderStatus? status, bool? isUnassigned, bool? isScheduled)
    {
        return await _context.MaintenanceOrders
            .Include(o => o.Elevator)
                .ThenInclude(el => el.Building)
            .Include(o => o.AssignedEngineer)
            .Include(o => o.Report)
            .Where(o => o.ScheduledDate.Year == year
                     && o.ScheduledDate.Month == month
                     && (companyId == null || o.Elevator.Building.CompanyId == companyId)
                     && (engineerId == null || o.AssignedEngineerId == engineerId)
                     && (status == null || o.Status == status)
                     && (isUnassigned == null || (isUnassigned == true ? o.AssignedEngineerId == null : o.AssignedEngineerId != null))
                     && (isScheduled == null || (isScheduled == true ? o.MaintenanceType == MaintenanceType.Scheduled : o.MaintenanceType == MaintenanceType.Unscheduled)))
            .OrderBy(o => o.Elevator.Building.Name)
            .ThenBy(o => o.Elevator.NumberInProject)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<MaintenanceOrder>> GetOrdersUntilDateAsync(DateTime untilDate)
    {
        return await _context.MaintenanceOrders
            .Include(o => o.Elevator)
                .ThenInclude(el => el.Building)
            .Include(o => o.Report)
            .Where(o => o.ScheduledDate <= untilDate)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<MaintenanceOrder>> GetUnscheduledOrdersAsync()
    {
        return await _context.MaintenanceOrders
            .Include(o => o.Elevator)
                .ThenInclude(el => el.Building)
            .Include(o => o.Report)
            .Where(o => o.MaintenanceType == MaintenanceType.Unscheduled && o.Status != OrderStatus.Done)
            .AsNoTracking()
            .ToListAsync();
    }
}
