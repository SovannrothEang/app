using CoreAPI.DTOs.Auth;
using CoreAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoreAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
[Tags("Roles")]
[RequireHttps]
[Authorize(Policy = Constants.TenantScopeAccessPolicy)]
public class RolesController(IRoleService roleService) : ControllerBase
{
    private readonly IRoleService _roleService = roleService;

    /// <summary>
    ///     Get all available roles
    /// </summary>
    /// <returns> A list of roles </returns>
    [HttpGet]
    // [Authorize(Policy = Constants.TenantAccessPolicy)]
    public ActionResult GetAll()
    {
        var roles = _roleService.GetAllRoles();
        return Ok(roles);
    }

    /// <summary>
    ///     Create role by role name via Admin access only
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<ActionResult> CreateRole([FromBody] RoleCreateDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _roleService.CreateRoleAsync(dto);
        return result.Succeeded
            ? Ok(new { message = $"Role '{dto.Name}' is created" })
            : BadRequest(result.Errors);
    }

    /// <summary>
    ///     Assign role to user via Admin access or Tenant owner, by using username and role name
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    [HttpPost("assign-user")]
    public async Task<ActionResult> AssignRole([FromBody] AssignRoleDto dto)
    {
        var result = await _roleService.AssignRoleAsync(dto);
        return result.Succeeded
            ? Ok(new { message = $"User {dto.UserName} added to role {dto.RoleName}" })
            : BadRequest(result.Errors);
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
