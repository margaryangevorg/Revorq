using Revorq.API.Models.BuildingModels;
using Revorq.API.Services.Interfaces;
using Revorq.DAL.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Revorq.API.Controllers;

[ApiController]
[Route("api/building")]
[Authorize(Roles = $"{nameof(Role.Admin)},{nameof(Role.Manager)}")]
public class BuildingController : ControllerBase
{
    private readonly IBuildingService _buildingService;

    public BuildingController(IBuildingService buildingService)
    {
        _buildingService = buildingService;
    }

    [HttpGet("all")]
    public async Task<IActionResult> GetAll([FromQuery] BuildingType? type)
    {
        return Ok(await _buildingService.GetAllAsync(type));
    }

    [HttpGet("byName/{name}")]
    public async Task<IActionResult> GetByName(string name)
    {
        var result = await _buildingService.GetByNameAsync(name);
        if (result.IsNotFound) return NotFound(result.ErrorMessage);
        return Ok(result.Data);
    }

    [HttpGet("byId/{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _buildingService.GetByIdAsync(id);
        if (result.IsNotFound) return NotFound(result.ErrorMessage);
        return Ok(result.Data);
    }

    [HttpPost]
    [Authorize(Roles = nameof(Role.Admin))]
    public async Task<IActionResult> Create([FromBody] BuildingRequest request)
    {
        var result = await _buildingService.CreateAsync(request);

        if (!result.IsSuccess) 
            return BadRequest(result.ErrorMessage);

        return Ok();
    }

    [HttpPut("{id}")]
    [Authorize(Roles = nameof(Role.Admin))]
    public async Task<IActionResult> Update(int id, [FromBody] BuildingRequest request)
    {
        var result = await _buildingService.UpdateAsync(id, request);
        if (result.IsNotFound) return NotFound(result.ErrorMessage);
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = nameof(Role.Admin))]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _buildingService.DeleteAsync(id);
        if (result.IsNotFound) return NotFound(result.ErrorMessage);
        return NoContent();
    }
}
