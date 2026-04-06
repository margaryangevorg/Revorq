using Revorq.API.Models;
using Revorq.API.Models.AuthModels;
using Revorq.DAL.Enums;

namespace Revorq.API.Services.Interfaces;

public interface IUserService
{
    Task<ServiceResult<UserResponse>> GetUserAsync(int userId);
    Task<ServiceResult<IEnumerable<UserResponse>>> GetCompanyUsersAsync(int companyId, Role? role);
    Task<ServiceResult<bool>> EditProfileAsync(int userId, EditProfileRequest request);
}
