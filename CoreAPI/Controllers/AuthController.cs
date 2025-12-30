using CoreAPI.DTOs.Auth;
using CoreAPI.DTOs.Customers;
using CoreAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoreAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
[Tags("Authentications")]
public class AuthController(
    IUserService userService,
    ICustomerService customerService,
    ICurrentUserProvider currentUser) : ControllerBase
{
    private readonly IUserService _userService = userService;
    private readonly ICustomerService _customerService = customerService;
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
    
    [HttpPost("register/customer")]
    [AllowAnonymous] // I think I'll need a route for this ???
    public async Task<ActionResult> CreateCustomerAsync(
        [FromBody] CustomerCreateDto dto,
        CancellationToken cancellationToken = default)
    {
        var customer = await _customerService.CreateAsync(dto, cancellationToken);
        return Ok(new
        {
            Message = "Register successfully!",
            UserName = dto.UserName,
            Email = customer.Email
        });
    }

    [HttpGet("me")]
    [Authorize(Policy = Constants.RequireAuthenticatedUser)]
    public ActionResult GetMe()
    {
        if (!_currentUser.IsAuthenticated)
            return Unauthorized();
         
        return Ok(new { _currentUser.UserId, _currentUser.Email, _currentUser.TenantId, _currentUser.Roles });
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
    
    [HttpPost("complete-invite")]
    [AllowAnonymous]
    public async Task<IActionResult> CompleteInvite([FromBody] SetupPasswordRequest req)
    {
        try 
        {
            await _userService.CompleteInviteAsync(req.UserId, req.Token, req.NewPassword);
            return Ok(new { Message = "Account activated." });
        }
        catch(Exception ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }
    
}
