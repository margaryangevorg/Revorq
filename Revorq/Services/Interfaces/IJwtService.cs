using Revorq.DAL.Entities;

namespace Revorq.API.Services.Interfaces;

public interface IJwtService
{
    Task<(string token, DateTime expiresAt)> GenerateAccessTokenAsync(AppUser user);
    (string token, DateTime expiresAt) GenerateRefreshToken();
}
