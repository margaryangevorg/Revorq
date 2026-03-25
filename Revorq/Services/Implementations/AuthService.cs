using Revorq.API.Models;
using Revorq.API.Models.AuthModels;
using Revorq.API.Services.Interfaces;
using Revorq.DAL.Entities;
using Microsoft.AspNetCore.Identity;

namespace Revorq.API.Services.Implementations;

public class AuthService : IAuthService
{
    private readonly UserManager<AppUser> _userManager;
    private readonly SignInManager<AppUser> _signInManager;
    private readonly IJwtService _jwtService;

    public AuthService(
        UserManager<AppUser> userManager,
        SignInManager<AppUser> signInManager,
        IJwtService jwtService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwtService = jwtService;
    }

    public async Task<ServiceResult<AuthResponse>> RegisterAsync(RegisterRequest request)
    {
        if (request.Password != request.ConfirmPassword)
            return ServiceResult<AuthResponse>.Error("Passwords do not match.");

        var user = new AppUser
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            UserName = request.Email
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
            return ServiceResult<AuthResponse>.Error(string.Join("; ", result.Errors.Select(e => e.Description)));

        var roleName = request.Role.ToString();
        await _userManager.AddToRoleAsync(user, roleName);
        var token = await _jwtService.GenerateTokenAsync(user);

        return ServiceResult<AuthResponse>.Ok(new AuthResponse
        {
            Token = token,
            Email = user.Email!,
            FullName = $"{user.FirstName} {user.LastName}",
            Role = roleName,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        });
    }

    public async Task<ServiceResult<AuthResponse>> LoginAsync(LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null)
            return ServiceResult<AuthResponse>.Error("Invalid email or password.");

        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: false);
        if (!result.Succeeded)
            return ServiceResult<AuthResponse>.Error("Invalid email or password.");

        var roles = await _userManager.GetRolesAsync(user);
        var token = await _jwtService.GenerateTokenAsync(user);

        return ServiceResult<AuthResponse>.Ok(new AuthResponse
        {
            Token = token,
            Email = user.Email!,
            FullName = $"{user.FirstName} {user.LastName}",
            Role = roles.FirstOrDefault() ?? string.Empty,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        });
    }
}
