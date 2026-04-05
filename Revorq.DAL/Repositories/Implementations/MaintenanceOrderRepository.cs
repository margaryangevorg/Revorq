using Revorq.DAL.Context;
using Revorq.DAL.Entities;
using Revorq.DAL.Enums;
using Revorq.DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Revorq.DAL.Repositories.Implementations;

public class MaintenanceOrderRepository : Repository<MaintenanceOrder>, IMaintenanceOrderRepository
{
    public MaintenanceOrderRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<MaintenanceOrder>> GetMonthlyOrdersAsync(
        int userId, int? engineerId, int year, int month, OrderStatus? status, bool? isUnassigned)
    {
        return await _context.MaintenanceOrders
            .Include(o => o.Elevator)
                .ThenInclude(el => el.Building)
            .Include(o => o.AssignedEngineer)
            .Include(o => o.Report)
            .Where(o => _context.UserBuildingAccesses.Any(a => a.UserId == userId && a.BuildingId == o.Elevator.BuildingId)
                     && o.ScheduledDate.Year == year
                     && o.ScheduledDate.Month == month
                     && (engineerId == null || o.AssignedEngineerId == engineerId)
                     && (status == null || o.Status == status)
                     && (isUnassigned == null || (isUnassigned == true ? o.AssignedEngineerId == null : o.AssignedEngineerId != null)))
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
