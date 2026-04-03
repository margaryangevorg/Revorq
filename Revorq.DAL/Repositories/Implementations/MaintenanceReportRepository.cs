using Revorq.DAL.Context;
using Revorq.DAL.Entities;
using Revorq.DAL.Repositories.Interfaces;

namespace Revorq.DAL.Repositories.Implementations;

public class MaintenanceReportRepository : Repository<MaintenanceReport>, IMaintenanceReportRepository
{
    public MaintenanceReportRepository(AppDbContext context) : base(context) { }
}
