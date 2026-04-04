using Revorq.DAL.Entities;
using Revorq.DAL.Enums;

namespace Revorq.DAL.Repositories.Interfaces;

public interface IBuildingRepository : IRepository<Building>
{
    Task<IEnumerable<Building>> GetAllAsync(int companyId, BuildingType? type);
    Task<Building?> GetWithElevatorsAsync(int buildingId, int companyId);
    Task<Building?> GetByNameAsync(string name, int companyId);
}
