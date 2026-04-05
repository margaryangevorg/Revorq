using Revorq.API.Models;
using Revorq.API.Models.BuildingModels;
using Revorq.DAL.Enums;

namespace Revorq.API.Services.Interfaces;

public interface IBuildingService
{
    Task<IEnumerable<BuildingResponse>> GetAllAsync(int userId, BuildingType? type = null);
    Task<ServiceResult<BuildingWithElevatorsResponse>> GetByIdAsync(int id, int companyId);
    Task<ServiceResult<BuildingWithElevatorsResponse>> GetByNameAsync(string name, int companyId);
    Task<ServiceResult<bool>> CreateAsync(BuildingRequest request, int companyId);
    Task<ServiceResult<bool>> UpdateAsync(int id, BuildingRequest request, int companyId);
    Task<ServiceResult<bool>> DeleteAsync(int id, int companyId);
}
