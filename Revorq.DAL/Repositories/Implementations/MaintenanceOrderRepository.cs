using Revorq.DAL.Context;
using Revorq.DAL.Entities;
using Revorq.DAL.Enums;
using Revorq.DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Revorq.DAL.Repositories.Implementations;

public class MaintenanceOrderRepository : Repository<MaintenanceOrder>, IMaintenanceOrderRepository
{
    public MaintenanceOrderRepository(AppDbContext context) : base(context) { }

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
        int? engineerId, int year, int month, OrderStatus? status, bool? isUnassigned, bool? isScheduled)
    {
        return await _context.MaintenanceOrders
            .Include(o => o.Elevator)
                .ThenInclude(el => el.Building)
            .Include(o => o.AssignedEngineer)
            .Include(o => o.Report)
            .Where(o => o.ScheduledDate.HasValue && o.ScheduledDate.Value.Year == year
                     && o.ScheduledDate.Value.Month == month
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
