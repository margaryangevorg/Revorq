using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Revorq.API.Models.DeviceModels;
using Revorq.API.Services.Interfaces;
using System.Security.Claims;

namespace Revorq.API.Controllers;

[ApiController]
[Route("api/devices")]
[Authorize]
public class DevicesController : ControllerBase
{
    private readonly IDeviceService _deviceService;

    public DevicesController(IDeviceService deviceService)
    {
        _deviceService = deviceService;
    }

    [HttpPost("push-token")]
    public async Task<IActionResult> RegisterPushToken([FromBody] RegisterPushTokenRequest request)
    {
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
            return Unauthorized();

        var result = await _deviceService.RegisterPushTokenAsync(userId, request);
        if (!result.IsSuccess) return BadRequest(result.ErrorMessage);

        return Ok();
    }

    [HttpDelete("push-token")]
    public async Task<IActionResult> RemovePushToken([FromBody] RemovePushTokenRequest request)
    {
        var result = await _deviceService.RemovePushTokenAsync(request);
        if (!result.IsSuccess) return BadRequest(result.ErrorMessage);

        return Ok();
    }
}
