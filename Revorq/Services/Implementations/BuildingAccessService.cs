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

    public async Task<ServiceResult<bool>> GrantAsync(int targetUserId, List<int> buildingIds, int requestingUserId)
    {
        var companyId = await GetCompanyIdAsync(requestingUserId);
        if (companyId is null)
            return ServiceResult<bool>.NotFound("Requesting user not found.");

        var targetUser = await _userManager.FindByIdAsync(targetUserId.ToString());
        if (targetUser is null || targetUser.CompanyId != companyId.Value)
            return ServiceResult<bool>.NotFound($"User {targetUserId} not found.");

        foreach (var buildingId in buildingIds)
        {
            var building = await _buildingRepository.GetByIdAsync(buildingId);
            if (building is null || building.CompanyId != companyId.Value)
                return ServiceResult<bool>.NotFound($"Building {buildingId} not found.");

            if (!await _accessRepository.ExistsAsync(targetUserId, buildingId))
                await _accessRepository.GrantAsync(targetUserId, buildingId);
        }

        await _accessRepository.SaveChangesAsync();

        return ServiceResult<bool>.Ok(true);
    }

    public async Task<ServiceResult<bool>> RevokeAsync(int targetUserId, List<int> buildingIds, int requestingUserId)
    {
        var companyId = await GetCompanyIdAsync(requestingUserId);
        if (companyId is null)
            return ServiceResult<bool>.NotFound("Requesting user not found.");

        foreach (var buildingId in buildingIds)
        {
            var building = await _buildingRepository.GetByIdAsync(buildingId);
            if (building is null || building.CompanyId != companyId.Value)
                return ServiceResult<bool>.NotFound($"Building {buildingId} not found.");

            if (await _accessRepository.ExistsAsync(targetUserId, buildingId))
                await _accessRepository.RevokeAsync(targetUserId, buildingId);
        }

        await _accessRepository.SaveChangesAsync();

        return ServiceResult<bool>.Ok(true);
    }

    public async Task<ServiceResult<IEnumerable<UserResponse>>> GetUsersWithAccessAsync(int buildingId, int requestingUserId)
    {
        var companyId = await GetCompanyIdAsync(requestingUserId);
        if (companyId is null)
            return ServiceResult<IEnumerable<UserResponse>>.NotFound("Requesting user not found.");

        var building = await _buildingRepository.GetByIdAsync(buildingId);
        if (building is null || building.CompanyId != companyId.Value)
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

    public async Task<ServiceResult<IEnumerable<BuildingResponse>>> GetBuildingsForUserAsync(int targetUserId, int requestingUserId)
    {
        var companyId = await GetCompanyIdAsync(requestingUserId);
        if (companyId is null)
            return ServiceResult<IEnumerable<BuildingResponse>>.NotFound("Requesting user not found.");

        var targetUser = await _userManager.FindByIdAsync(targetUserId.ToString());
        if (targetUser is null || targetUser.CompanyId != companyId.Value)
            return ServiceResult<IEnumerable<BuildingResponse>>.NotFound($"User {targetUserId} not found.");

        var buildings = await _accessRepository.GetBuildingsForUserAsync(targetUserId);

        var responses = buildings.Select(b => new BuildingResponse
        {
            Id = b.Id,
            Name = b.Name,
            Address = b.Address,
            BuildingType = b.BuildingType,
            Latitude = b.Latitude,
            Longitude = b.Longitude,
            ElevatorCount = b.Elevators.Count
        });

        return ServiceResult<IEnumerable<BuildingResponse>>.Ok(responses);
    }

    private async Task<int?> GetCompanyIdAsync(int userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        return user?.CompanyId;
    }
}
