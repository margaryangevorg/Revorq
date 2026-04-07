using Revorq.API.Models;
using Revorq.API.Models.ElevatorModels;

namespace Revorq.API.Services.Interfaces;

public interface IElevatorService
{
    Task<IEnumerable<ElevatorResponse>> GetAllAsync(int userId);
    Task<IEnumerable<ElevatorResponse>> GetByBuildingNameAsync(string buildingName, int userId);
    Task<ServiceResult<ElevatorResponse>> GetByIdAsync(int id, int userId);
    Task<ServiceResult<bool>> CreateAsync(ElevatorRequest request, int userId);
    Task<ServiceResult<bool>> UpdateAsync(int id, ElevatorRequest request, int userId);
    Task<ServiceResult<bool>> DeleteAsync(int id, int userId);
}
