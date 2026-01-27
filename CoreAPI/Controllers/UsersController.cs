using CoreAPI.DTOs;
using CoreAPI.DTOs.Auth;
using CoreAPI.Services.Interfaces;
using CoreAPI.Validators.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoreAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
[RequireHttps]
[Tags("Users")]
public class UsersController(IUserService userService, IRoleService roleService) : ControllerBase
{
    private readonly IUserService _userService = userService;
    private readonly IRoleService _roleService = roleService;

    /// <summary>
    /// Get all users, in the scope of the tenant (Global filtering)
    /// </summary>
    [HttpGet]
    [Authorize(Policy = Constants.TenantScopeAccessPolicy)]
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
    [Authorize(Policy = Constants.TenantScopeAccessPolicy)]
    public async Task<ActionResult> GetUserByIdAsync(
        string id,
        CancellationToken ct = default)
    {
        var user = await _userService.GetUserByIdAsync(id, ct);
        return Ok(user);
    }

    /// <summary>
    /// Get all users, in the scope of the tenant (Global filtering)
    /// </summary>
    [HttpGet("admin")]
    [Authorize(Policy = Constants.PlatformRootAccessPolicy)]
    public async Task<ActionResult> GetAllUsersByAdminAsync(
        [FromQuery] PaginationOption option,
        CancellationToken ct = default)
    {
        var users = await _userService.GetPagedResultForAdminAsync(option, ct);
        return Ok(users);
    }

    /// <summary>
    /// Get user by id in the scope of tenant
    /// </summary>
    [HttpGet("admin/{id}")]
    [Authorize(Policy = Constants.PlatformRootAccessPolicy)]
    public async Task<ActionResult> GetUserByIdByAdminAsync(
        string id,
        CancellationToken ct = default)
    {
        var user = await _userService.GetUserByIdForAdminAsync(id, ct);
        return Ok(user);
    }
    
    /// <summary>
    /// Register an account for a user by superadmin or tenant
    /// </summary>
    [HttpPost]
    [Authorize(Policy = Constants.TenantScopeAccessPolicy)]
    [EndpointDescription("This endpoint will act the registration endpoint for admin, action is done by SuperAdmin only")]
    public async Task<ActionResult> CreateAsync([FromBody] OnboardingUserDto dto)
    {
        var validator = new OnBoardingUserDtoValidator(_roleService);
        var result = await validator.ValidateAsync(dto);
        if (!result.IsValid)
            return BadRequest(result.Errors);
        
        var (userId, token) = await _userService.OnboardingUserAsync(dto);
        return Ok( new
        {
            UserId = userId,
            Token = token,
        });
    }

}
