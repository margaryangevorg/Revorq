using Revorq.API.Models;
using Revorq.API.Models.DeviceModels;
using Revorq.API.Services.Interfaces;
using Revorq.DAL.Entities;
using Revorq.DAL.Repositories.Interfaces;

namespace Revorq.API.Services.Implementations;

public class DeviceService : IDeviceService
{
    private static readonly HashSet<string> ValidPlatforms = new(StringComparer.OrdinalIgnoreCase) { "android", "ios" };

    private readonly IDeviceTokenRepository _deviceTokenRepository;

    public DeviceService(IDeviceTokenRepository deviceTokenRepository)
    {
        _deviceTokenRepository = deviceTokenRepository;
    }

    public async Task<ServiceResult<bool>> RegisterPushTokenAsync(int userId, RegisterPushTokenRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Token))
            return ServiceResult<bool>.Error("Token is required.");

        if (!ValidPlatforms.Contains(request.Platform))
            return ServiceResult<bool>.Error("Platform must be 'android' or 'ios'.");

        var existing = await _deviceTokenRepository.GetByTokenAsync(request.Token);
        if (existing is null)
        {
            await _deviceTokenRepository.AddAsync(new DeviceToken
            {
                UserId = userId,
                Token = request.Token,
                Platform = request.Platform.ToLowerInvariant()
            });
        }
        else
        {
            existing.UserId = userId;
            existing.Platform = request.Platform.ToLowerInvariant();
            _deviceTokenRepository.Update(existing);
        }

        await _deviceTokenRepository.SaveChangesAsync();
        return ServiceResult<bool>.Ok(true);
    }

    public async Task<ServiceResult<bool>> RemovePushTokenAsync(RemovePushTokenRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Token))
            return ServiceResult<bool>.Error("Token is required.");

        await _deviceTokenRepository.DeleteByTokenAsync(request.Token);
        await _deviceTokenRepository.SaveChangesAsync();
        return ServiceResult<bool>.Ok(true);
    }
}
