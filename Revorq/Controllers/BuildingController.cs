using Revorq.API.Models.BuildingModels;
using Revorq.API.Services.Interfaces;
using Revorq.DAL.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

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
        var companyId = GetCompanyId();
        if (companyId is null) return Unauthorized();

        return Ok(await _buildingService.GetAllAsync(companyId.Value, type));
    }

    [HttpGet("byName/{name}")]
    public async Task<IActionResult> GetByName(string name)
    {
        var companyId = GetCompanyId();
        if (companyId is null) return Unauthorized();

        var result = await _buildingService.GetByNameAsync(name, companyId.Value);
        if (result.IsNotFound) return NotFound(result.ErrorMessage);
        return Ok(result.Data);
    }

    [HttpGet("byId/{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var companyId = GetCompanyId();
        if (companyId is null) return Unauthorized();

        var result = await _buildingService.GetByIdAsync(id, companyId.Value);
        if (result.IsNotFound) return NotFound(result.ErrorMessage);
        return Ok(result.Data);
    }

    [HttpPost]
    [Authorize(Roles = nameof(Role.Admin))]
    public async Task<IActionResult> Create([FromBody] BuildingRequest request)
    {
        var companyId = GetCompanyId();
        if (companyId is null) return Unauthorized();

        var result = await _buildingService.CreateAsync(request, companyId.Value);
        if (!result.IsSuccess)
            return BadRequest(result.ErrorMessage);

        return Ok();
    }

    [HttpPut("{id}")]
    [Authorize(Roles = nameof(Role.Admin))]
    public async Task<IActionResult> Update(int id, [FromBody] BuildingRequest request)
    {
        var companyId = GetCompanyId();
        if (companyId is null) return Unauthorized();

        var result = await _buildingService.UpdateAsync(id, request, companyId.Value);
        if (result.IsNotFound) return NotFound(result.ErrorMessage);
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = nameof(Role.Admin))]
    public async Task<IActionResult> Delete(int id)
    {
        var companyId = GetCompanyId();
        if (companyId is null) return Unauthorized();

        var result = await _buildingService.DeleteAsync(id, companyId.Value);
        if (result.IsNotFound) return NotFound(result.ErrorMessage);
        return NoContent();
    }

    private int? GetCompanyId()
    {
        var claim = User.FindFirstValue("CompanyId");
        return int.TryParse(claim, out var id) ? id : null;
    }
}
