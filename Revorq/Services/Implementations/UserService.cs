using Revorq.API.Models;
using Revorq.API.Models.AuthModels;
using Revorq.API.Services.Interfaces;
using Revorq.DAL.Entities;
using Revorq.DAL.Enums;
using Revorq.DAL.Repositories.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace Revorq.API.Services.Implementations;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly ICompanyRepository _companyRepository;
    private readonly UserManager<AppUser> _userManager;

    public UserService(IUserRepository userRepository, ICompanyRepository companyRepository, UserManager<AppUser> userManager)
    {
        _userRepository = userRepository;
        _companyRepository = companyRepository;
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
            CompanyName = user.Company.Name,
            CompanyLogoUrl = user.Company.LogoUrl
        });
    }

    public async Task<ServiceResult<IEnumerable<UserResponse>>> GetCompanyUsersAsync(int companyId, Role? role)
    {
        var company = await _companyRepository.GetByIdAsync(companyId);
        if (company is null)
            return ServiceResult<IEnumerable<UserResponse>>.NotFound("Company not found.");

        var users = await _userRepository.GetCompanyUsersByRoleAsync(companyId, role?.ToString());

        var responses = new List<UserResponse>();
        foreach (var u in users)
        {
            var userRole = role?.ToString()
                ?? (await _userManager.GetRolesAsync(u)).FirstOrDefault()
                ?? string.Empty;

            responses.Add(new UserResponse
            {
                Id = u.Id,
                Username = u.UserName ?? string.Empty,
                FullName = $"{u.FirstName} {u.LastName}",
                Email = u.Email ?? string.Empty,
                Phone = u.PhoneNumber,
                Role = userRole,
                CompanyId = u.CompanyId,
                CompanyName = company.Name
            });
        }

        return ServiceResult<IEnumerable<UserResponse>>.Ok(responses);
    }

    public async Task<ServiceResult<bool>> EditProfileAsync(int userId, EditProfileRequest request)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
            return ServiceResult<bool>.NotFound("User not found.");

        if (!string.Equals(user.UserName, request.Username, StringComparison.OrdinalIgnoreCase))
        {
            var existing = await _userManager.FindByNameAsync(request.Username);
            if (existing is not null)
                return ServiceResult<bool>.Error("Username is already taken.");

            user.UserName = request.Username;
        }

        user.FirstName = request.FirstName;
        user.LastName = request.LastName;
        user.PhoneNumber = request.Phone;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
            return ServiceResult<bool>.Error(string.Join("; ", result.Errors.Select(e => e.Description)));

        return ServiceResult<bool>.Ok(true);
    }

    public async Task<ServiceResult<bool>> ChangePasswordAsync(int userId, ChangePasswordRequest request)
    {
        if (request.NewPassword != request.ConfirmedPassword)
            return ServiceResult<bool>.Error("Passwords do not match.");

        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
            return ServiceResult<bool>.NotFound("User not found.");

        var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
        if (!result.Succeeded)
            return ServiceResult<bool>.Error(string.Join("; ", result.Errors.Select(e => e.Description)));

        return ServiceResult<bool>.Ok(true);
    }

    public async Task<ServiceResult<bool>> DeleteUserAsync(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user is null)
            return ServiceResult<bool>.NotFound("User not found.");

        user.Status = EntityStatus.Deleted;
        await _userRepository.SaveChangesAsync();

        return ServiceResult<bool>.Ok(true);
    }
}
