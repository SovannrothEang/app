using CoreAPI.DTOs;
using CoreAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoreAPI.Controllers;

[Route("api/admin/[controller]")]
[ApiController]
[RequireHttps]
[Tags("Users")]
[Authorize(Policy = Constants.PlatformRootAccessPolicy)]
public class UsersController(IUserService userService) : ControllerBase
{
    private readonly IUserService _userService = userService;
    
    /// <summary>
    /// Get all users, in the scope of the tenant (Global filtering)
    /// </summary>
    [HttpGet]
    public async Task<ActionResult> GetAllUsers(
        [FromQuery] PaginationOption option,
        CancellationToken ct = default)
    {
        var users = await _userService.GetPagedResultAsync(option, ct);
        return Ok(users);
    }

    /// <summary>
    /// Get user by id in the scope of tenant
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult> GetUserById(string id, CancellationToken ct = default)
    {
        var user = await _userService.GetUserById(id, ct);
        return Ok(user);
    }
}
