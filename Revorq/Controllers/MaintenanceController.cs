using Revorq.API.Models.MaintenanceOrderModels;
using Revorq.API.Services.Interfaces;
using Revorq.DAL.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

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
    public async Task<IActionResult> GetMonthly(
        [FromQuery] int? engineerId,
        [FromQuery] int year,
        [FromQuery] int month,
        [FromQuery] OrderStatus? status,
        [FromQuery] bool? isUnassigned)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        return Ok(await _maintenanceService.GetMonthlyAsync(userId.Value, engineerId, year, month, status, isUnassigned));
    }

    [HttpGet("unscheduled")]
    [Authorize(Roles = $"{nameof(Role.Admin)},{nameof(Role.Manager)}")]
    public async Task<IActionResult> GetUnscheduled()
    {
        return Ok(await _maintenanceService.GetUnscheduledAsync());
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
    {
        var result = await _maintenanceService.CreateOrderAsync(request);
        if (!result.IsSuccess) return BadRequest(result.ErrorMessage);
        return Ok();
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _maintenanceService.GetByIdAsync(id);
        if (result.IsNotFound) return NotFound(result.ErrorMessage);
        return Ok(result.Data);
    }

    [HttpPut("{orderId}/assign")]
    [Authorize(Roles = $"{nameof(Role.Admin)},{nameof(Role.Manager)}")]
    public async Task<IActionResult> AssignOrder(int orderId, int engineerId)
    {
        var result = await _maintenanceService.AssignOrderAsync(orderId, engineerId);
        if (result.IsNotFound) return NotFound(result.ErrorMessage);
        if (!result.IsSuccess) return BadRequest(result.ErrorMessage);
        return Ok();
    }

    [HttpPost("{id}/report")]
    [Authorize(Roles = $"{nameof(Role.Admin)},{nameof(Role.Manager)},{nameof(Role.MaintenanceEngineer)}")]
    public async Task<IActionResult> CreateReport(int id, [FromForm] CreateReportRequest request)
    {
        var result = await _maintenanceService.CreateReportAsync(id, request);
        if (result.IsNotFound) return NotFound(result.ErrorMessage);
        if (!result.IsSuccess) return BadRequest(result.ErrorMessage);
        return Ok();
    }

    private int? GetUserId()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(claim, out var id) ? id : null;
    }

    private int? GetCompanyId()
    {
        var claim = User.FindFirstValue("CompanyId");
        return int.TryParse(claim, out var id) ? id : null;
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = nameof(Role.Admin))]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _maintenanceService.DeleteAsync(id);
        if (result.IsNotFound) return NotFound(result.ErrorMessage);
        return Ok();
    }
}
