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
            CompanyName = user.Company.Name
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
}
