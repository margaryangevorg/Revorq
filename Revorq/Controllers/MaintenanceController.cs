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
        [FromQuery] int year,
        [FromQuery] int month,
        [FromQuery] OrderStatus? status,
        [FromQuery] bool? isUnassigned,
        [FromQuery] bool? isScheduled)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        return Ok(await _maintenanceService.GetMonthlyAsync(userId.Value, year, month, status, isUnassigned, isScheduled));
    }

    [HttpGet("unscheduled")]
    [Authorize(Roles = $"{nameof(Role.Admin)},{nameof(Role.Manager)}")]
    public async Task<IActionResult> GetUnscheduled()
    {
        return Ok(await _maintenanceService.GetUnscheduledAsync());
    }

    [HttpPost("default-planning")]
    [Authorize(Roles = $"{nameof(Role.Admin)},{nameof(Role.Manager)}")]
    public async Task<IActionResult> CreateDefaultPlanning([FromQuery] int year, [FromQuery] int month)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var result = await _maintenanceService.CreateDefaultPlanningAsync(userId.Value, year, month);
        if (result.IsNotFound) return NotFound(result.ErrorMessage);
        if (!result.IsSuccess) return BadRequest(result.ErrorMessage);
        return Ok(result.Data);
    }

    [HttpPost("auto-planning")]
    [Authorize(Roles = $"{nameof(Role.Admin)},{nameof(Role.Manager)}")]
    public async Task<IActionResult> AutoPlanning([FromQuery] int year, [FromQuery] int month)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var result = await _maintenanceService.AutoPlanningAsync(userId.Value, year, month);
        if (!result.IsSuccess) return BadRequest(result.ErrorMessage);
        return Ok(result.Data);
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromForm] CreateOrderRequest request)
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

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateOrder(int id, [FromBody] UpdateOrderRequest request)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var result = await _maintenanceService.UpdateOrderAsync(id, request, userId.Value);
        if (result.IsNotFound) return NotFound(result.ErrorMessage);
        if (!result.IsSuccess) return Forbid();
        return Ok();
    }

    [HttpPost("{id}/order/images")]
    public async Task<IActionResult> AddOrderImages(int id, [FromForm] List<IFormFile> images)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var result = await _maintenanceService.AddOrderImagesAsync(id, images, userId.Value);
        if (result.IsNotFound) return NotFound(result.ErrorMessage);
        if (!result.IsSuccess) return Forbid();
        return Ok();
    }

    [HttpDelete("{id}/order/images")]
    public async Task<IActionResult> DeleteOrderImages(int id, [FromBody] List<string> imageUrls)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var result = await _maintenanceService.DeleteOrderImagesAsync(id, imageUrls, userId.Value);
        if (result.IsNotFound) return NotFound(result.ErrorMessage);
        if (!result.IsSuccess) return Forbid();
        return Ok();
    }

    [HttpPost("{id}/report/images")]
    [Authorize(Roles = $"{nameof(Role.Admin)},{nameof(Role.Manager)}")]
    public async Task<IActionResult> AddReportImages(int id, [FromForm] List<IFormFile> images)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var result = await _maintenanceService.AddReportImagesAsync(id, images, userId.Value);
        if (result.IsNotFound) return NotFound(result.ErrorMessage);
        if (!result.IsSuccess) return Forbid();
        return Ok();
    }

    [HttpDelete("{id}/report/images")]
    [Authorize(Roles = $"{nameof(Role.Admin)},{nameof(Role.Manager)}")]
    public async Task<IActionResult> DeleteReportImages(int id, [FromBody] List<string> imageUrls)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var result = await _maintenanceService.DeleteReportImagesAsync(id, imageUrls, userId.Value);
        if (result.IsNotFound) return NotFound(result.ErrorMessage);
        if (!result.IsSuccess) return Forbid();
        return Ok();
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

    [HttpPut("{id}/report")]
    [Authorize(Roles = $"{nameof(Role.Admin)},{nameof(Role.Manager)}")]
    public async Task<IActionResult> UpdateReport(int id, [FromBody] UpdateReportRequest request)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var result = await _maintenanceService.UpdateReportAsync(id, request, userId.Value);
        if (result.IsNotFound) return NotFound(result.ErrorMessage);
        if (!result.IsSuccess) return Forbid();
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

    [HttpDelete("{id}")]
    [Authorize(Roles = nameof(Role.Admin))]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _maintenanceService.DeleteAsync(id);
        if (result.IsNotFound) return NotFound(result.ErrorMessage);
        return Ok();
    }

    private int? GetUserId()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(claim, out var id) ? id : null;
    }
}
