using Revorq.API.Models;
using Revorq.API.Models.ElevatorModels;

namespace Revorq.API.Services.Interfaces;

public interface IElevatorService
{
    Task<IEnumerable<ElevatorResponse>> GetAllAsync();
    Task<ServiceResult<ElevatorResponse>> GetByIdAsync(int id);
    Task<ServiceResult<bool>> CreateAsync(ElevatorRequest request);
    Task<ServiceResult<bool>> UpdateAsync(int id, ElevatorRequest request);
    Task<ServiceResult<bool>> DeleteAsync(int id);
}
