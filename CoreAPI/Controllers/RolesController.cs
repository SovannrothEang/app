using CoreAPI.DTOs.Auth;
using CoreAPI.Services.Interfaces;
using CoreAPI.Validators.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoreAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
[Tags("Roles")]
[RequireHttps]
[Authorize(Policy = Constants.TenantScopeAccessPolicy)]
public class RolesController(IRoleService roleService, ILogger<RolesController> logger) : ControllerBase
{
    private readonly IRoleService _roleService = roleService;
    private readonly ILogger<RolesController> _logger = logger;

    /// <summary>
    /// Get all available roles
    /// </summary>
    [HttpGet]
    // [Authorize(Policy = Constants.TenantAccessPolicy)]
    public ActionResult GetAll()
    {
        var roles = _roleService.GetAllRoles();
        return Ok(roles);
    }

    /// <summary>
    /// Create role by role name via Admin access only
    /// </summary>
    [HttpPost]
    public async Task<ActionResult> CreateRole([FromBody] RoleCreateDto dto)
    {
        var result = new RoleCreateDtoValidator().Validate(dto);
        if (!result.IsValid)
        {
            if (_logger.IsEnabled(LogLevel.Warning))
                _logger.LogWarning("Role creation failed due to invalid data: {Errors}", result.Errors);
            return BadRequest(result.Errors);
        }

        var roleCreated = await _roleService.CreateRoleAsync(dto);
        if (!roleCreated.Succeeded)
        {
            if (_logger.IsEnabled(LogLevel.Warning))
                _logger.LogWarning("Role creation failed: {Errors}", roleCreated.Errors);
            return BadRequest(roleCreated.Errors);
        }

        return Ok(new { message = $"Role '{dto.Name}' is created" });
    }

    /// <summary>
    /// Assign role to user via Admin access or Tenant owner, by using username and role name
    /// </summary>
    [HttpPost("assign-user")]
    public async Task<ActionResult> AssignRole([FromBody] AssignRoleDto dto)
    {
        var result = new AssignRoleDtoValidator().Validate(dto);
        if (!result.IsValid)
        {
            if (_logger.IsEnabled(LogLevel.Warning))
                _logger.LogWarning("Role assignment failed due to invalid data: {Errors}", result.Errors);
            return BadRequest(result.Errors);
        }

        var roleResult = await _roleService.AssignRoleAsync(dto);
        if (!roleResult.Succeeded)
        {
            if (_logger.IsEnabled(LogLevel.Warning))
                _logger.LogWarning("Role assignment failed: {Errors}", roleResult.Errors);
            return BadRequest(roleResult.Errors);
        }

        return Ok(new { message = $"User {dto.UserName} added to role {dto.RoleName}" });
    }

    [HttpPost("unassign-user")]
    public ActionResult UnassignRole([FromBody] AssignRoleDto dto)
    {
        // TODO: Add logic for unassign user from specific role
        return Ok();
    }
    
    [HttpPut("{roleId}")]
    public ActionResult UpdateRoleAsync(string roleId)
    {
        // TODO: Update role's info
        return Ok();
    }
    
    [HttpDelete("{roleId}")]
    public ActionResult RemoveRoleAsync(string roleId)
    {
        // TODO: delete role by id, soft delete
        return Ok();
    }
}
