using CoreAPI.DTOs.Auth;
using CoreAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoreAPI.Controllers;

[Route("api/admin/[controller]")]
[ApiController]
[Tags("Roles")]
[Authorize(Policy = Constants.RequireSuperAdminRole)]
public class RolesController(IRoleService roleService) : ControllerBase
{
    private readonly IRoleService _roleService = roleService;

    /// <summary>
    ///     Get all available roles
    /// </summary>
    /// <returns> A list of roles </returns>
    [HttpGet()]
    [Authorize(Policy = Constants.RequireSuperAdminRole)]
    public ActionResult GetAll()
    {
        var roles = _roleService.GetAllRoles();
        return Ok();
    }

    /// <summary>
    ///     Create role by role name via Admin access only
    /// </summary>
    /// <param name="roleCreate"></param>
    /// <returns></returns>
    [HttpPost()]
    [Authorize(Policy = Constants.RequireSuperAdminRole)]
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
    [Authorize(Policy = Constants.RequireSuperAdminRole)]
    public async Task<ActionResult> AssignRole([FromBody] AssignRoleDto dto)
    {
        var result = await _roleService.AssignRoleAsync(dto);
        return result.Succeeded
            ? Ok(new { message = $"User {dto.UserName} added to role {dto.RoleName}" })
            : BadRequest(result.Errors);
    }
}
