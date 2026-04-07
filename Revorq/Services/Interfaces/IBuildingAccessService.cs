using Revorq.API.Models;
using Revorq.API.Models.AuthModels;
using Revorq.API.Models.BuildingModels;

namespace Revorq.API.Services.Interfaces;

public interface IBuildingAccessService
{
    Task<ServiceResult<bool>> GrantAsync(int targetUserId, List<int> buildingIds, int requestingUserId);
    Task<ServiceResult<bool>> RevokeAsync(int targetUserId, List<int> buildingIds, int requestingUserId);
    Task<ServiceResult<IEnumerable<UserResponse>>> GetUsersWithAccessAsync(int buildingId, int requestingUserId);
    Task<ServiceResult<IEnumerable<BuildingResponse>>> GetBuildingsForUserAsync(int targetUserId, int requestingUserId);
}
