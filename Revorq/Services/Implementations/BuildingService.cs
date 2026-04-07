using Revorq.API.Models;
using Revorq.API.Models.BuildingModels;
using Revorq.API.Services.Interfaces;
using Revorq.DAL.Entities;
using Revorq.DAL.Enums;
using Revorq.DAL.Repositories.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace Revorq.API.Services.Implementations;

public class BuildingService : IBuildingService
{
    private readonly IBuildingRepository _repository;
    private readonly IUserBuildingAccessRepository _accessRepository;
    private readonly UserManager<AppUser> _userManager;

    public BuildingService(
        IBuildingRepository repository,
        IUserBuildingAccessRepository accessRepository,
        UserManager<AppUser> userManager)
    {
        _repository = repository;
        _accessRepository = accessRepository;
        _userManager = userManager;
    }

    public async Task<IEnumerable<BuildingResponse>> GetAllAsync(int userId, BuildingType? type = null)
    {
        var buildings = await _accessRepository.GetBuildingsForUserAsync(userId);
        return buildings
            .Where(b => type == null || b.BuildingType == type)
            .Select(b => new BuildingResponse
            {
                Id = b.Id,
                Name = b.Name,
                Address = b.Address,
                BuildingType = b.BuildingType,
                ElevatorCount = b.Elevators.Count
            });
    }

    public async Task<ServiceResult<BuildingWithElevatorsResponse>> GetByNameAsync(string name, int userId)
    {
        var companyId = await GetCompanyIdAsync(userId);
        if (companyId is null)
            return ServiceResult<BuildingWithElevatorsResponse>.NotFound("User not found.");

        var building = await _repository.GetByNameAsync(name, companyId.Value);
        if (building is null)
            return ServiceResult<BuildingWithElevatorsResponse>.NotFound($"Building '{name}' not found.");

        return ServiceResult<BuildingWithElevatorsResponse>.Ok(MapToWithElevatorsResponse(building));
    }

    public async Task<ServiceResult<BuildingWithElevatorsResponse>> GetByIdAsync(int id, int userId)
    {
        var companyId = await GetCompanyIdAsync(userId);
        if (companyId is null)
            return ServiceResult<BuildingWithElevatorsResponse>.NotFound("User not found.");

        var building = await _repository.GetWithElevatorsAsync(id, companyId.Value);
        if (building is null)
            return ServiceResult<BuildingWithElevatorsResponse>.NotFound($"Building {id} not found.");

        return ServiceResult<BuildingWithElevatorsResponse>.Ok(MapToWithElevatorsResponse(building));
    }

    public async Task<ServiceResult<bool>> CreateAsync(BuildingRequest request, int userId)
    {
        var companyId = await GetCompanyIdAsync(userId);
        if (companyId is null)
            return ServiceResult<bool>.NotFound("User not found.");

        var existing = await _repository.GetByNameAsync(request.Name, companyId.Value);
        if (existing is not null)
            return ServiceResult<bool>.Error("The building name already exists.");

        var building = new Building
        {
            Name = request.Name,
            Address = request.Address,
            BuildingType = request.BuildingType,
            CompanyId = companyId.Value
        };

        await _repository.AddAsync(building);
        await _repository.SaveChangesAsync();

        var admins = (await _userManager.GetUsersInRoleAsync(nameof(Role.Admin)))
            .Where(u => u.CompanyId == companyId.Value);

        foreach (var admin in admins)
            await _accessRepository.GrantAsync(admin.Id, building.Id);

        await _accessRepository.SaveChangesAsync();

        return ServiceResult<bool>.Ok(true);
    }

    public async Task<ServiceResult<bool>> UpdateAsync(int id, BuildingRequest request, int userId)
    {
        var companyId = await GetCompanyIdAsync(userId);
        if (companyId is null)
            return ServiceResult<bool>.NotFound("User not found.");

        var building = await _repository.GetWithElevatorsAsync(id, companyId.Value);
        if (building is null)
            return ServiceResult<bool>.NotFound($"Building {id} not found.");

        building.Name = request.Name;
        building.Address = request.Address;
        building.BuildingType = request.BuildingType;

        _repository.Update(building);
        await _repository.SaveChangesAsync();

        return ServiceResult<bool>.Ok(true);
    }

    public async Task<ServiceResult<bool>> DeleteAsync(int id, int userId)
    {
        var companyId = await GetCompanyIdAsync(userId);
        if (companyId is null)
            return ServiceResult<bool>.NotFound("User not found.");

        var building = await _repository.GetWithElevatorsAsync(id, companyId.Value);
        if (building is null)
            return ServiceResult<bool>.NotFound($"Building {id} not found.");

        _repository.Delete(building);
        await _repository.SaveChangesAsync();

        return ServiceResult<bool>.Ok(true);
    }

    private async Task<int?> GetCompanyIdAsync(int userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        return user?.CompanyId;
    }

    private static BuildingWithElevatorsResponse MapToWithElevatorsResponse(Building building) => new()
    {
        Id = building.Id,
        Name = building.Name,
        Address = building.Address,
        BuildingType = building.BuildingType,
        Elevators = building.Elevators.Select(el => new ElevatorSummary
        {
            Id = el.Id,
            NumberInProject = el.NumberInProject,
            SerialNumber = el.SerialNumber
        }).ToList()
    };
}
