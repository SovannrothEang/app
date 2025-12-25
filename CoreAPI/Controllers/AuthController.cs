using CoreAPI.DTOs.Auth;
using CoreAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoreAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
[Tags("Authentications")]
public class AuthController(IUserService userService,
    ICurrentUserProvider currentUser) : ControllerBase
{
    private readonly IUserService _userService = userService;
    private readonly ICurrentUserProvider _currentUser = currentUser;

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult> Register([FromBody] RegisterDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _userService.RegisterAsync(dto);
        return result.Succeeded
            ? Ok(new { message = "User registered successfully" })
            : BadRequest("Email is already exist!");
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult> Login(
        [FromBody] LoginDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _userService.LoginAsync(dto);

        return result is null 
            ? BadRequest("Invalid credentials!")
            : Ok(result);
    }

    [HttpGet("me")]
    [Authorize(Policy = Constants.RequireAuthenticatedUser)]
    public IActionResult GetMe()
    {
        if (!_currentUser.IsAuthenticated)
            return Unauthorized();
         
        return Ok(new { _currentUser.UserId, _currentUser.Email, _currentUser.Roles });
    }

    [HttpPost("change-password")]
    [Authorize(Policy = Constants.RequireAuthenticatedUser)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordCommand command)
    {
        if (!_currentUser.IsAuthenticated)
            return Unauthorized();

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _userService.ChangePasswordAsync(_currentUser.UserId!, command);

        return result.Succeeded
            ? Ok(new { message = "Changed successfully." })
            : BadRequest(new { message = "Failed to change password. Please check your current password." });
    }
}
