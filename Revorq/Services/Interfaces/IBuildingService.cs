using Revorq.API.Models;
using Revorq.API.Models.BuildingModels;

namespace Revorq.API.Services.Interfaces;

public interface IBuildingService
{
    Task<IEnumerable<BuildingResponse>> GetAllAsync();
    Task<ServiceResult<BuildingWithElevatorsResponse>> GetByIdAsync(int id);
    Task<ServiceResult<BuildingWithElevatorsResponse>> GetByNameAsync(string name);
    Task<ServiceResult<bool>> CreateAsync(BuildingRequest request);
    Task<ServiceResult<bool>> UpdateAsync(int id, BuildingRequest request);
    Task<ServiceResult<bool>> DeleteAsync(int id);
}
