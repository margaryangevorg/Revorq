using Revorq.DAL.Entities;

namespace Revorq.API.Services.Interfaces;

public interface IJwtService
{
    Task<string> GenerateTokenAsync(AppUser user);
}
