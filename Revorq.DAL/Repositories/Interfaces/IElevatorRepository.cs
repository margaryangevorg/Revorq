using Revorq.DAL.Entities;

namespace Revorq.DAL.Repositories.Interfaces;

public interface IElevatorRepository : IRepository<Elevator>
{
    Task<Elevator?> GetBySerialNumberAsync(string serialNumber);
    Task<Elevator?> GetByNumberInProjectAsync(int buildingId, string numberInProject);
    Task<Elevator?> GetWithBuildingAsync(int id);
    Task<IEnumerable<Elevator>> GetAllByCompanyAsync(int companyId);
    Task<IEnumerable<Elevator>> GetByBuildingNameAsync(string buildingName, int companyId);
}
