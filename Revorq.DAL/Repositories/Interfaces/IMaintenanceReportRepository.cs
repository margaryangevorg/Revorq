using Revorq.DAL.Entities;

namespace Revorq.DAL.Repositories.Interfaces;

public interface IMaintenanceReportRepository : IRepository<MaintenanceReport>
{
    new Task<MaintenanceReport?> GetByIdAsync(long id);
}
