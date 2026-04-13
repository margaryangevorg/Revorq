using Revorq.API.Models;
using Revorq.API.Models.ElevatorModels;
using Revorq.API.Services.Interfaces;
using Revorq.DAL.Entities;
using Revorq.DAL.Enums;
using Revorq.DAL.Repositories.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace Revorq.API.Services.Implementations;

public class ElevatorService : IElevatorService
{
    private readonly IElevatorRepository _elevatorRepository;
    private readonly IBuildingRepository _buildingRepository;
    private readonly UserManager<AppUser> _userManager;

    public ElevatorService(
        IElevatorRepository elevatorRepository,
        IBuildingRepository buildingRepository,
        UserManager<AppUser> userManager)
    {
        _elevatorRepository = elevatorRepository;
        _buildingRepository = buildingRepository;
        _userManager = userManager;
    }

    public async Task<IEnumerable<ElevatorResponse>> GetAllAsync(int userId)
    {
        var elevators = await _elevatorRepository.GetAllByUserAsync(userId);
        return elevators.Select(MapToResponse);
    }

    public async Task<IEnumerable<ElevatorResponse>> GetByBuildingNameAsync(string buildingName, int userId)
    {
        var elevators = await _elevatorRepository.GetByBuildingNameByUserAsync(buildingName, userId);
        return elevators.Select(MapToResponse);
    }

    public async Task<ServiceResult<ElevatorResponse>> GetByIdAsync(int id, int userId)
    {
        var companyId = await GetCompanyIdAsync(userId);
        if (companyId is null)
            return ServiceResult<ElevatorResponse>.NotFound("User not found.");

        var elevator = await _elevatorRepository.GetWithBuildingAsync(id);
        if (elevator is null || elevator.Building.CompanyId != companyId.Value)
            return ServiceResult<ElevatorResponse>.NotFound($"Elevator {id} not found.");

        return ServiceResult<ElevatorResponse>.Ok(MapToResponse(elevator));
    }

    public async Task<ServiceResult<bool>> CreateAsync(ElevatorRequest request, int userId)
    {
        var companyId = await GetCompanyIdAsync(userId);
        if (companyId is null)
            return ServiceResult<bool>.NotFound("User not found.");

        var building = await _buildingRepository.GetByIdAsync(request.BuildingId);
        if (building is null || building.CompanyId != companyId.Value)
            return ServiceResult<bool>.Error($"Building {request.BuildingId} not found.");

        if (await _elevatorRepository.GetBySerialNumberAsync(request.SerialNumber) is not null)
            return ServiceResult<bool>.Error("An elevator with this serial number already exists.");

        if (await _elevatorRepository.GetByNumberInProjectAsync(request.BuildingId, request.NumberInProject) is not null)
            return ServiceResult<bool>.Error("An elevator with this number in project already exists for this building.");

        var elevator = new Elevator
        {
            NumberInProject = request.NumberInProject,
            SerialNumber = request.SerialNumber,
            Model = request.Model,
            ProductionCountry = request.ProductionCountry,
            CustomerFullName = request.CustomerFullName,
            CustomerPhoneNumber = request.CustomerPhoneNumber,
            WarrantyType = request.WarrantyType,
            WarrantyDate = request.WarrantyDate,
            Priority = request.Priority,
            BuildingId = request.BuildingId
        };

        await _elevatorRepository.AddAsync(elevator);
        await _elevatorRepository.SaveChangesAsync();

        return ServiceResult<bool>.Ok(true);
    }

    public async Task<ServiceResult<bool>> UpdateAsync(int id, ElevatorRequest request, int userId)
    {
        var companyId = await GetCompanyIdAsync(userId);
        if (companyId is null)
            return ServiceResult<bool>.NotFound("User not found.");

        var elevator = await _elevatorRepository.GetWithBuildingAsync(id);
        if (elevator is null || elevator.Building.CompanyId != companyId.Value)
            return ServiceResult<bool>.NotFound($"Elevator {id} not found.");

        var building = await _buildingRepository.GetByIdAsync(request.BuildingId);
        if (building is null || building.CompanyId != companyId.Value)
            return ServiceResult<bool>.Error($"Building {request.BuildingId} not found.");

        elevator.NumberInProject = request.NumberInProject;
        elevator.SerialNumber = request.SerialNumber;
        elevator.Model = request.Model;
        elevator.ProductionCountry = request.ProductionCountry;
        elevator.CustomerFullName = request.CustomerFullName;
        elevator.CustomerPhoneNumber = request.CustomerPhoneNumber;
        elevator.WarrantyType = request.WarrantyType;
        elevator.WarrantyDate = request.WarrantyDate;
        elevator.Priority = request.Priority;
        elevator.BuildingId = request.BuildingId;

        _elevatorRepository.Update(elevator);
        await _elevatorRepository.SaveChangesAsync();

        return ServiceResult<bool>.Ok(true);
    }

    public async Task<ServiceResult<bool>> DeleteAsync(int id, int userId)
    {
        var companyId = await GetCompanyIdAsync(userId);
        if (companyId is null)
            return ServiceResult<bool>.NotFound("User not found.");

        var elevator = await _elevatorRepository.GetWithBuildingAsync(id);
        if (elevator is null || elevator.Building.CompanyId != companyId.Value)
            return ServiceResult<bool>.NotFound($"Elevator {id} not found.");

        elevator.Status = EntityStatus.Deleted;
        _elevatorRepository.Update(elevator);
        await _elevatorRepository.SaveChangesAsync();

        return ServiceResult<bool>.Ok(true);
    }

    private async Task<int?> GetCompanyIdAsync(int userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        return user?.CompanyId;
    }

    private static ElevatorResponse MapToResponse(Elevator el) => new()
    {
        Id = el.Id,
        NumberInProject = el.NumberInProject,
        SerialNumber = el.SerialNumber,
        ElevatorModel = el.Model,
        ElevatorProductionCountry = el.ProductionCountry,
        CustomerFullName = el.CustomerFullName,
        CustomerPhoneNumber = el.CustomerPhoneNumber,
        WarrantyType = el.WarrantyType,
        WarrantyDate = el.WarrantyDate,
        Priority = el.Priority,
        CreationDate = el.CreationDate,
        BuildingId = el.BuildingId,
        BuildingName = el.Building?.Name ?? string.Empty
    };
}
