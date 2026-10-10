using Revorq.API.Models;
using Revorq.API.Models.DeviceModels;

namespace Revorq.API.Services.Interfaces;

public interface IDeviceService
{
    Task<ServiceResult<bool>> RegisterPushTokenAsync(int userId, RegisterPushTokenRequest request);
    Task<ServiceResult<bool>> RemovePushTokenAsync(RemovePushTokenRequest request);
}
