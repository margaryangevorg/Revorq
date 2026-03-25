using Revorq.API.Models.MaintenanceOrderModels;
using Revorq.API.Services.Interfaces;
using Revorq.DAL.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Revorq.API.Controllers;

[ApiController]
[Route("api/maintenance")]
[Authorize]
public class MaintenanceController : ControllerBase
{
    private readonly IMaintenanceService _maintenanceService;

    public MaintenanceController(IMaintenanceService maintenanceService)
    {
        _maintenanceService = maintenanceService;
    }

    [HttpGet("elevators")]
    [Authorize(Roles = $"{nameof(Role.Admin)},{nameof(Role.Manager)}")]
    public async Task<IActionResult> GetElevatorsUnderMaintenance([FromQuery] DateTime untilDate)
    {
        return Ok(await _maintenanceService.GetOrdersUntilDateAsync(untilDate));
    }

    [HttpGet("monthly")]
    [Authorize(Roles = $"{nameof(Role.Admin)},{nameof(Role.Manager)},{nameof(Role.MaintenanceEngineer)}")]
    public async Task<IActionResult> GetMonthlyList(
        [FromQuery] int engineerId,
        [FromQuery] int year,
        [FromQuery] int month)
    {
        return Ok(await _maintenanceService.GetMonthlyListAsync(engineerId, year, month));
    }

    [HttpGet("unscheduled")]
    [Authorize(Roles = $"{nameof(Role.Admin)},{nameof(Role.Manager)}")]
    public async Task<IActionResult> GetUnscheduled()
    {
        return Ok(await _maintenanceService.GetUnscheduledAsync());
    }

    [HttpPost]
    [Authorize(Roles = $"{nameof(Role.Admin)},{nameof(Role.Manager)}")]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
    {
        var result = await _maintenanceService.CreateOrderAsync(request);
        if (!result.IsSuccess) return BadRequest(result.ErrorMessage);
        return CreatedAtAction(nameof(GetById), new { id = result.Data }, result.Data);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _maintenanceService.GetByIdAsync(id);
        if (result.IsNotFound) return NotFound(result.ErrorMessage);
        return Ok(result.Data);
    }

    [HttpPut("{id}/complete")]
    [Authorize(Roles = $"{nameof(Role.Admin)},{nameof(Role.Manager)},{nameof(Role.MaintenanceEngineer)}")]
    public async Task<IActionResult> CompleteOrder(int id, [FromBody] CompleteOrderRequest request)
    {
        var result = await _maintenanceService.CompleteOrderAsync(id, request);
        if (result.IsNotFound) return NotFound(result.ErrorMessage);
        if (!result.IsSuccess) return BadRequest(result.ErrorMessage);
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = nameof(Role.Admin))]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _maintenanceService.DeleteAsync(id);
        if (result.IsNotFound) return NotFound(result.ErrorMessage);
        return NoContent();
    }
}
