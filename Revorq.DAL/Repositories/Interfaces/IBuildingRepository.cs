using Revorq.DAL.Entities;

namespace Revorq.DAL.Repositories.Interfaces;

public interface IBuildingRepository : IRepository<Building>
{
    Task<Building?> GetWithElevatorsAsync(int buildingId);
    Task<Building?> GetByNameAsync(string name);
}
