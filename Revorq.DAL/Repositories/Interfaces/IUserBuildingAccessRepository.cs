using Revorq.DAL.Entities;

namespace Revorq.DAL.Repositories.Interfaces;

public interface IUserBuildingAccessRepository
{
    Task<IEnumerable<AppUser>> GetUsersWithAccessAsync(int buildingId);
    Task<IEnumerable<Building>> GetBuildingsForUserAsync(int userId);
    Task<bool> ExistsAsync(int userId, int buildingId);
    Task GrantAsync(int userId, int buildingId);
    Task RevokeAsync(int userId, int buildingId);
    Task SaveChangesAsync();
}
