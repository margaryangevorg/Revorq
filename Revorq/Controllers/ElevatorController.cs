using Revorq.API.Models.ElevatorModels;
using Revorq.API.Services.Interfaces;
using Revorq.DAL.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Revorq.API.Controllers;

[ApiController]
[Route("api/elevator")]
[Authorize]
public class ElevatorController : ControllerBase
{
    private readonly IElevatorService _elevatorService;

    public ElevatorController(IElevatorService elevatorService)
    {
        _elevatorService = elevatorService;
    }

    [HttpGet("all")]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await _elevatorService.GetAllAsync());
    }

    [HttpGet("byBuildingName")]
    public async Task<IActionResult> GetByBuildingName([FromQuery] string buildingName)
    {
        return Ok(await _elevatorService.GetByBuildingNameAsync(buildingName));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _elevatorService.GetByIdAsync(id);
        if (result.IsNotFound) return NotFound(result.ErrorMessage);
        return Ok(result.Data);
    }

    [HttpPost]
    [Authorize(Roles = $"{nameof(Role.Admin)}")]
    public async Task<IActionResult> Create([FromBody] ElevatorRequest request)
    {
        var result = await _elevatorService.CreateAsync(request);

        if (!result.IsSuccess)
            return BadRequest(result.ErrorMessage);

        return Ok();
    }

    [HttpPut("{id}")]
    [Authorize(Roles = $"{nameof(Role.Admin)},{nameof(Role.Manager)}")]
    public async Task<IActionResult> Update(int id, [FromBody] ElevatorRequest request)
    {
        var result = await _elevatorService.UpdateAsync(id, request);
        if (result.IsNotFound) return NotFound(result.ErrorMessage);
        if (!result.IsSuccess) return BadRequest(result.ErrorMessage);
        return Ok();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = nameof(Role.Admin))]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _elevatorService.DeleteAsync(id);
        if (result.IsNotFound) return NotFound(result.ErrorMessage);
        return Ok();
    }
}
