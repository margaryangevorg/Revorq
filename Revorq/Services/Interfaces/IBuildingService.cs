using Revorq.API.Models;
using Revorq.API.Models.BuildingModels;
using Revorq.DAL.Enums;

namespace Revorq.API.Services.Interfaces;

public interface IBuildingService
{
    Task<IEnumerable<BuildingResponse>> GetAllAsync(int userId, BuildingType? type = null);
    Task<ServiceResult<BuildingWithElevatorsResponse>> GetByIdAsync(int id, int userId);
    Task<ServiceResult<BuildingWithElevatorsResponse>> GetByNameAsync(string name, int userId);
    Task<ServiceResult<bool>> CreateAsync(BuildingRequest request, int userId);
    Task<ServiceResult<bool>> UpdateAsync(int id, BuildingUpdateRequest request, int userId);
    Task<ServiceResult<bool>> AddFilesAsync(int buildingId, List<IFormFile> files, int userId);
    Task<ServiceResult<bool>> DeleteFilesAsync(int buildingId, List<string> fileUrls, int userId);
    Task<ServiceResult<bool>> DeleteAsync(int id, int userId);
}
