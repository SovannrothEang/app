using CoreAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoreAPI.Controllers;

[Route("api/admin/[controller]")]
[ApiController]
[Tags("Users")]
[Authorize(Policy = Constants.PlatformRootAccessPolicy)]
public class UsersController(IUserService userService) : ControllerBase
{
    private readonly IUserService _userService = userService;

    [HttpGet]
    public async Task<ActionResult> GetAllUsers(CancellationToken ct = default)
    {
        var users = await _userService.GetAllUserAsync(ct);
        return Ok(users);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult> GetUserById(string id, CancellationToken ct = default)
    {
        var user = await _userService.GetUserById(id, ct);
        return Ok(user);
    }
}
