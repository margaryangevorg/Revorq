using Revorq.API.Models;
using Revorq.API.Models.ElevatorModels;
using Revorq.API.Services.Interfaces;
using Revorq.DAL.Entities;
using Revorq.DAL.Repositories.Interfaces;

namespace Revorq.API.Services.Implementations;

public class ElevatorService : IElevatorService
{
    private readonly IRepository<Elevator> _elevatorRepository;
    private readonly IBuildingRepository _buildingRepository;

    public ElevatorService(
        IRepository<Elevator> elevatorRepository,
        IBuildingRepository buildingRepository)
    {
        _elevatorRepository = elevatorRepository;
        _buildingRepository = buildingRepository;
    }

    public async Task<IEnumerable<ElevatorResponse>> GetAllAsync()
    {
        var elevators = await _elevatorRepository.GetAllAsync();
        return elevators.Select(el => MapToResponse(el));
    }

    public async Task<ServiceResult<ElevatorResponse>> GetByIdAsync(int id)
    {
        var elevator = await _elevatorRepository.GetByIdAsync(id);
        if (elevator is null)
            return ServiceResult<ElevatorResponse>.NotFound($"Elevator {id} not found.");

        return ServiceResult<ElevatorResponse>.Ok(MapToResponse(elevator));
    }

    public async Task<ServiceResult<bool>> CreateAsync(ElevatorRequest request)
    {
        var building = await _buildingRepository.GetByIdAsync(request.BuildingId);
        if (building is null)
            return ServiceResult<bool>.Error($"Building {request.BuildingId} not found.");

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
            BuildingId = request.BuildingId
        };

        await _elevatorRepository.AddAsync(elevator);
        await _elevatorRepository.SaveChangesAsync();

        return ServiceResult<bool>.Ok(true);
    }

    public async Task<ServiceResult<bool>> UpdateAsync(int id, ElevatorRequest request)
    {
        var elevator = await _elevatorRepository.GetByIdAsync(id);
        if (elevator is null)
            return ServiceResult<bool>.NotFound($"Elevator {id} not found.");

        var building = await _buildingRepository.GetByIdAsync(request.BuildingId);
        if (building is null)
            return ServiceResult<bool>.Error($"Building {request.BuildingId} not found.");

        elevator.NumberInProject = request.NumberInProject;
        elevator.SerialNumber = request.SerialNumber;
        elevator.Model = request.Model;
        elevator.ProductionCountry = request.ProductionCountry;
        elevator.CustomerFullName = request.CustomerFullName;
        elevator.CustomerPhoneNumber = request.CustomerPhoneNumber;
        elevator.WarrantyType = request.WarrantyType;
        elevator.WarrantyDate = request.WarrantyDate;
        elevator.BuildingId = request.BuildingId;

        _elevatorRepository.Update(elevator);
        await _elevatorRepository.SaveChangesAsync();

        return ServiceResult<bool>.Ok(true);
    }

    public async Task<ServiceResult<bool>> DeleteAsync(int id)
    {
        var elevator = await _elevatorRepository.GetByIdAsync(id);
        if (elevator is null)
            return ServiceResult<bool>.NotFound($"Elevator {id} not found.");

        _elevatorRepository.Delete(elevator);
        await _elevatorRepository.SaveChangesAsync();

        return ServiceResult<bool>.Ok(true);
    }

    private static ElevatorResponse MapToResponse(Elevator el, string? buildingName = null) => new()
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
        CreationDate = el.CreationDate,
        BuildingId = el.BuildingId,
        BuildingName = buildingName ?? el.Building?.Name ?? string.Empty
    };
}
