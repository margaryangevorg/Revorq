using Revorq.API.Models;
using Revorq.API.Models.AuthModels;
using Revorq.API.Services.Interfaces;
using Revorq.DAL.Entities;
using Revorq.DAL.Repositories.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace Revorq.API.Services.Implementations;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly UserManager<AppUser> _userManager;

    public UserService(IUserRepository userRepository, UserManager<AppUser> userManager)
    {
        _userRepository = userRepository;
        _userManager = userManager;
    }

    public async Task<ServiceResult<UserResponse>> GetUserAsync(int userId)
    {
        var user = await _userRepository.GetUserWithCompanyAsync(userId);
        if (user is null)
            return ServiceResult<UserResponse>.NotFound("User not found.");

        var roles = await _userManager.GetRolesAsync(user);

        return ServiceResult<UserResponse>.Ok(new UserResponse
        {
            Id = user.Id,
            Username = user.UserName ?? string.Empty,
            FullName = $"{user.FirstName} {user.LastName}",
            Email = user.Email ?? string.Empty,
            Phone = user.PhoneNumber,
            Role = roles.FirstOrDefault() ?? string.Empty,
            CompanyId = user.CompanyId,
            CompanyName = user.Company.Name
        });
    }
}
