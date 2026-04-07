using Revorq.API.Models.BuildingModels;
using Revorq.API.Services.Interfaces;
using Revorq.DAL.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Revorq.API.Controllers;

[ApiController]
[Route("api/building")]
[Authorize]
public class BuildingController : ControllerBase
{
    private readonly IBuildingService _buildingService;
    private readonly IBuildingAccessService _accessService;

    public BuildingController(IBuildingService buildingService, IBuildingAccessService accessService)
    {
        _buildingService = buildingService;
        _accessService = accessService;
    }

    [HttpGet("all")]
    public async Task<IActionResult> GetAll([FromQuery] BuildingType? type)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        return Ok(await _buildingService.GetAllAsync(userId.Value, type));
    }

    [HttpGet("byName/{name}")]
    public async Task<IActionResult> GetByName(string name)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var result = await _buildingService.GetByNameAsync(name, userId.Value);
        if (result.IsNotFound) return NotFound(result.ErrorMessage);
        return Ok(result.Data);
    }

    [HttpGet("byId/{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var result = await _buildingService.GetByIdAsync(id, userId.Value);
        if (result.IsNotFound) return NotFound(result.ErrorMessage);
        return Ok(result.Data);
    }

    [HttpPost]
    [Authorize(Roles = nameof(Role.Admin))]
    public async Task<IActionResult> Create([FromBody] BuildingRequest request)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var result = await _buildingService.CreateAsync(request, userId.Value);
        if (!result.IsSuccess)
            return BadRequest(result.ErrorMessage);

        return Ok();
    }

    [HttpPut("{id}")]
    [Authorize(Roles = nameof(Role.Admin))]
    public async Task<IActionResult> Update(int id, [FromBody] BuildingRequest request)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var result = await _buildingService.UpdateAsync(id, request, userId.Value);
        if (result.IsNotFound) return NotFound(result.ErrorMessage);
        return Ok();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = nameof(Role.Admin))]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var result = await _buildingService.DeleteAsync(id, userId.Value);
        if (result.IsNotFound) return NotFound(result.ErrorMessage);
        return Ok();
    }

    [HttpPost("access/{targetUserId}")]
    [Authorize(Roles = nameof(Role.Admin))]
    public async Task<IActionResult> GrantAccess(int targetUserId, [FromBody] List<int> buildingIds)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var result = await _accessService.GrantAsync(targetUserId, buildingIds, userId.Value);
        if (result.IsNotFound) return NotFound(result.ErrorMessage);
        if (!result.IsSuccess) return BadRequest(result.ErrorMessage);
        return Ok();
    }

    [HttpDelete("access/{targetUserId}")]
    [Authorize(Roles = nameof(Role.Admin))]
    public async Task<IActionResult> RevokeAccess(int targetUserId, [FromBody] List<int> buildingIds)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var result = await _accessService.RevokeAsync(targetUserId, buildingIds, userId.Value);
        if (result.IsNotFound) return NotFound(result.ErrorMessage);
        return Ok();
    }

    [HttpGet("{buildingId}/access")]
    public async Task<IActionResult> GetUsersWithAccess(int buildingId)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var result = await _accessService.GetUsersWithAccessAsync(buildingId, userId.Value);
        if (result.IsNotFound) return NotFound(result.ErrorMessage);
        return Ok(result.Data);
    }

    [HttpGet("access/user/{targetUserId}")]
    public async Task<IActionResult> GetBuildingsForUser(int targetUserId)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var result = await _accessService.GetBuildingsForUserAsync(targetUserId, userId.Value);
        if (result.IsNotFound) return NotFound(result.ErrorMessage);
        return Ok(result.Data);
    }

    private int? GetUserId()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(claim, out var id) ? id : null;
    }
}
