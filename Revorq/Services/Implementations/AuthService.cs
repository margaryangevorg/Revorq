using Revorq.API.Models;
using Revorq.API.Models.AuthModels;
using Revorq.API.Services.Interfaces;
using Revorq.DAL.Entities;
using Revorq.DAL.Enums;
using Revorq.DAL.Repositories.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace Revorq.API.Services.Implementations;

public class AuthService : IAuthService
{
    private readonly UserManager<AppUser> _userManager;
    private readonly SignInManager<AppUser> _signInManager;
    private readonly IJwtService _jwtService;
    private readonly ICompanyRepository _companyRepository;

    public AuthService(
        UserManager<AppUser> userManager,
        SignInManager<AppUser> signInManager,
        IJwtService jwtService,
        ICompanyRepository companyRepository)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwtService = jwtService;
        _companyRepository = companyRepository;
    }

    public async Task<ServiceResult<AuthResponse>> LoginAsync(LoginRequest request)
    {
        var user = await _userManager.FindByNameAsync(request.Username);
        if (user is null)
            return ServiceResult<AuthResponse>.Error("Invalid username or password.");

        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: false);
        if (!result.Succeeded)
            return ServiceResult<AuthResponse>.Error("Invalid username or password.");

        var company = await _companyRepository.GetByIdAsync(user.CompanyId);
        if (company is null)
            return ServiceResult<AuthResponse>.Error("Company not found.");

        if (company.Status == CompanyStatus.Pending)
            return ServiceResult<AuthResponse>.Error("Your company registration is awaiting approval.");

        if (company.Status == CompanyStatus.Rejected)
            return ServiceResult<AuthResponse>.Error("Your company registration was rejected.");

        var roles = await _userManager.GetRolesAsync(user);
        var token = await _jwtService.GenerateTokenAsync(user);

        return ServiceResult<AuthResponse>.Ok(new AuthResponse
        {
            Token = token,
            Email = user.Email ?? string.Empty,
            FullName = $"{user.FirstName} {user.LastName}",
            Role = roles.FirstOrDefault() ?? string.Empty,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        });
    }
}
