using Revorq.API.Models;
using Revorq.API.Models.BuildingModels;
using Revorq.API.Services.Interfaces;
using Revorq.DAL.Entities;
using Revorq.DAL.Repositories.Interfaces;

namespace Revorq.API.Services.Implementations;

public class BuildingService : IBuildingService
{
    private readonly IBuildingRepository _repository;

    public BuildingService(IBuildingRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<BuildingResponse>> GetAllAsync()
    {
        var buildings = await _repository.GetAllAsync();
        return buildings.Select(b => new BuildingResponse
        {
            Id = b.Id,
            Name = b.Name,
            Address = b.Address,
            BuildingType = b.BuildingType,
            ElevatorCount = b.Elevators.Count
        });
    }

    public async Task<ServiceResult<BuildingWithElevatorsResponse>> GetByNameAsync(string name)
    {
        var building = await _repository.GetByNameAsync(name);
        if (building is null)
            return ServiceResult<BuildingWithElevatorsResponse>.NotFound($"Building '{name}' not found.");

        return ServiceResult<BuildingWithElevatorsResponse>.Ok(new BuildingWithElevatorsResponse
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
        });
    }

    public async Task<ServiceResult<BuildingWithElevatorsResponse>> GetByIdAsync(int id)
    {
        var building = await _repository.GetWithElevatorsAsync(id);
        if (building is null)
            return ServiceResult<BuildingWithElevatorsResponse>.NotFound($"Building {id} not found.");

        return ServiceResult<BuildingWithElevatorsResponse>.Ok(new BuildingWithElevatorsResponse
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
        });
    }

    public async Task<ServiceResult<bool>> CreateAsync(BuildingRequest request)
    {
        var existing = await _repository.GetByNameAsync(request.Name);
        if (existing is not null)
            return ServiceResult<bool>.Error("The building name already exists.");

        var building = new Building
        {
            Name = request.Name,
            Address = request.Address,
            BuildingType = request.BuildingType
        };

        await _repository.AddAsync(building);
        await _repository.SaveChangesAsync();

        return ServiceResult<bool>.Ok(true);
    }

    public async Task<ServiceResult<bool>> UpdateAsync(int id, BuildingRequest request)
    {
        var building = await _repository.GetByIdAsync(id);
        if (building is null)
            return ServiceResult<bool>.NotFound($"Building {id} not found.");

        building.Name = request.Name;
        building.Address = request.Address;
        building.BuildingType = request.BuildingType;

        _repository.Update(building);
        await _repository.SaveChangesAsync();

        return ServiceResult<bool>.Ok(true);
    }

    public async Task<ServiceResult<bool>> DeleteAsync(int id)
    {
        var building = await _repository.GetByIdAsync(id);
        if (building is null)
            return ServiceResult<bool>.NotFound($"Building {id} not found.");

        _repository.Delete(building);
        await _repository.SaveChangesAsync();

        return ServiceResult<bool>.Ok(true);
    }
}
