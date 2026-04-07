using Revorq.API.Models;
using Revorq.API.Models.AuthModels;
using Revorq.API.Models.BuildingModels;

namespace Revorq.API.Services.Interfaces;

public interface IBuildingAccessService
{
    Task<ServiceResult<bool>> GrantAsync(int userId, List<int> buildingIds, int companyId);
    Task<ServiceResult<bool>> RevokeAsync(int buildingId, int userId, int companyId);
    Task<ServiceResult<IEnumerable<UserResponse>>> GetUsersWithAccessAsync(int buildingId, int companyId);
    Task<ServiceResult<IEnumerable<BuildingResponse>>> GetBuildingsForUserAsync(int userId, int companyId);
}
