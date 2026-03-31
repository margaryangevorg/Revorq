using Revorq.DAL.Entities;

namespace Revorq.DAL.Repositories.Interfaces;

public interface IElevatorRepository : IRepository<Elevator>
{
    Task<Elevator?> GetBySerialNumberAsync(string serialNumber);
    Task<Elevator?> GetByNumberInProjectAsync(int buildingId, string numberInProject);
}
