using Revorq.API.Models;
using Revorq.API.Models.AuthModels;

namespace Revorq.API.Services.Interfaces;

public interface IUserService
{
    Task<ServiceResult<UserResponse>> GetUserAsync(int userId);
}
