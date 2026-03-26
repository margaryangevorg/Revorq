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
    private readonly IRefreshTokenRepository _refreshTokenRepository;

    public AuthService(
        UserManager<AppUser> userManager,
        SignInManager<AppUser> signInManager,
        IJwtService jwtService,
        ICompanyRepository companyRepository,
        IRefreshTokenRepository refreshTokenRepository)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwtService = jwtService;
        _companyRepository = companyRepository;
        _refreshTokenRepository = refreshTokenRepository;
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
        return ServiceResult<AuthResponse>.Ok(await BuildAuthResponseAsync(user, roles.FirstOrDefault() ?? string.Empty));
    }

    public async Task<ServiceResult<AuthResponse>> RefreshAsync(string refreshToken)
    {
        var stored = await _refreshTokenRepository.GetByTokenAsync(refreshToken);

        if (stored is null || stored.IsRevoked)
            return ServiceResult<AuthResponse>.Error("Invalid refresh token.");

        if (stored.ExpiresAt < DateTime.Now)
            return ServiceResult<AuthResponse>.Error("Refresh token has expired.");

        // Revoke the used token (rotation)
        stored.IsRevoked = true;
        _refreshTokenRepository.Update(stored);
        await _refreshTokenRepository.SaveChangesAsync();

        var roles = await _userManager.GetRolesAsync(stored.User);
        return ServiceResult<AuthResponse>.Ok(await BuildAuthResponseAsync(stored.User, roles.FirstOrDefault() ?? string.Empty));
    }

    private async Task<AuthResponse> BuildAuthResponseAsync(AppUser user, string role)
    {
        var (accessToken, accessExpiresAt) = await _jwtService.GenerateAccessTokenAsync(user);
        var (refreshToken, refreshExpiresAt) = _jwtService.GenerateRefreshToken();

        await _refreshTokenRepository.AddAsync(new RefreshToken
        {
            Token = refreshToken,
            UserId = user.Id,
            ExpiresAt = refreshExpiresAt
        });
        await _refreshTokenRepository.SaveChangesAsync();

        return new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            Email = user.Email ?? string.Empty,
            FullName = $"{user.FirstName} {user.LastName}",
            Role = role,
            AccessTokenExpiresAt = accessExpiresAt,
            RefreshTokenExpiresAt = refreshExpiresAt
        };
    }
}
