using Revorq.DAL.Entities;
using Revorq.DAL.Enums;

namespace Revorq.DAL.Repositories.Interfaces;

public interface IBuildingRepository : IRepository<Building>
{
    Task<IEnumerable<Building>> GetAllAsync(BuildingType? type);
    Task<Building?> GetWithElevatorsAsync(int buildingId);
    Task<Building?> GetByNameAsync(string name);
}
