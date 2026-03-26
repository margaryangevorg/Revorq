using Revorq.API.Models;
using Revorq.API.Models.AuthModels;

namespace Revorq.API.Services.Interfaces;

public interface IAuthService
{
    Task<ServiceResult<AuthResponse>> LoginAsync(LoginRequest request);
    Task<ServiceResult<AuthResponse>> RefreshAsync(string refreshToken);
}
