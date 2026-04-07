using Revorq.API.Models;
using Revorq.API.Models.AuthModels;
using Revorq.API.Models.BuildingModels;
using Revorq.API.Services.Interfaces;
using Revorq.DAL.Repositories.Interfaces;
using Microsoft.AspNetCore.Identity;
using Revorq.DAL.Entities;

namespace Revorq.API.Services.Implementations;

public class BuildingAccessService : IBuildingAccessService
{
    private readonly IUserBuildingAccessRepository _accessRepository;
    private readonly IBuildingRepository _buildingRepository;
    private readonly UserManager<AppUser> _userManager;

    public BuildingAccessService(
        IUserBuildingAccessRepository accessRepository,
        IBuildingRepository buildingRepository,
        UserManager<AppUser> userManager)
    {
        _accessRepository = accessRepository;
        _buildingRepository = buildingRepository;
        _userManager = userManager;
    }

    public async Task<ServiceResult<bool>> GrantAsync(int userId, List<int> buildingIds, int companyId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null || user.CompanyId != companyId)
            return ServiceResult<bool>.NotFound($"User {userId} not found.");

        foreach (var buildingId in buildingIds)
        {
            var building = await _buildingRepository.GetByIdAsync(buildingId);
            if (building is null || building.CompanyId != companyId)
                return ServiceResult<bool>.NotFound($"Building {buildingId} not found.");

            if (!await _accessRepository.ExistsAsync(userId, buildingId))
                await _accessRepository.GrantAsync(userId, buildingId);
        }

        await _accessRepository.SaveChangesAsync();

        return ServiceResult<bool>.Ok(true);
    }

    public async Task<ServiceResult<bool>> RevokeAsync(int userId, List<int> buildingIds, int companyId)
    {
        foreach (var buildingId in buildingIds)
        {
            var building = await _buildingRepository.GetByIdAsync(buildingId);
            if (building is null || building.CompanyId != companyId)
                return ServiceResult<bool>.NotFound($"Building {buildingId} not found.");

            if (await _accessRepository.ExistsAsync(userId, buildingId))
                await _accessRepository.RevokeAsync(userId, buildingId);
        }

        await _accessRepository.SaveChangesAsync();

        return ServiceResult<bool>.Ok(true);
    }

    public async Task<ServiceResult<IEnumerable<UserResponse>>> GetUsersWithAccessAsync(int buildingId, int companyId)
    {
        var building = await _buildingRepository.GetByIdAsync(buildingId);
        if (building is null || building.CompanyId != companyId)
            return ServiceResult<IEnumerable<UserResponse>>.NotFound($"Building {buildingId} not found.");

        var users = await _accessRepository.GetUsersWithAccessAsync(buildingId);

        var responses = new List<UserResponse>();
        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            responses.Add(new UserResponse
            {
                Id = user.Id,
                Username = user.UserName ?? string.Empty,
                FullName = $"{user.FirstName} {user.LastName}",
                Email = user.Email ?? string.Empty,
                Phone = user.PhoneNumber,
                Role = roles.FirstOrDefault() ?? string.Empty,
                CompanyId = user.CompanyId
            });
        }

        return ServiceResult<IEnumerable<UserResponse>>.Ok(responses);
    }

    public async Task<ServiceResult<IEnumerable<BuildingResponse>>> GetBuildingsForUserAsync(int userId, int companyId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null || user.CompanyId != companyId)
            return ServiceResult<IEnumerable<BuildingResponse>>.NotFound($"User {userId} not found.");

        var buildings = await _accessRepository.GetBuildingsForUserAsync(userId);

        var responses = buildings.Select(b => new BuildingResponse
        {
            Id = b.Id,
            Name = b.Name,
            Address = b.Address,
            BuildingType = b.BuildingType,
            ElevatorCount = b.Elevators.Count
        });

        return ServiceResult<IEnumerable<BuildingResponse>>.Ok(responses);
    }
}
