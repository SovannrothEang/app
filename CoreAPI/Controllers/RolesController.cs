using CoreAPI.DTOs.Auth;
using CoreAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoreAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
[Tags("Roles")]
[Authorize(Policy = Constants.TenantAccessPolicy)]
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
    /// <param name="roleCreate"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<ActionResult> CreateRole([FromBody] RoleCreateDto roleCreate)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _roleService.CreateRoleAsync(roleCreate);
        return result.Succeeded
            ? Ok(new { message = $"Role '{roleCreate.Name}' is created" })
            : BadRequest(result.Errors);
    }

    /// <summary>
    ///     Assign role to user via Admin access only, by using username and rolename
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
        return Ok();
    }
    
    [HttpPut("{roleId}")]
    public ActionResult UpdateRoleAsync(string roleId)
    {
        return Ok();
    }
    
    [HttpDelete("{roleId}")]
    public ActionResult RemoveRoleAsync(string roleId)
    {
        return Ok();
    }
}
