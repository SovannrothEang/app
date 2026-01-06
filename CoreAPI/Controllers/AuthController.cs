using CoreAPI.DTOs.Auth;
using CoreAPI.DTOs.Customers;
using CoreAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoreAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
[Tags("Authentications")]
[RequireHttps]
public class AuthController(
    IUserService userService,
    ICustomerService customerService,
    ICurrentUserProvider currentUser) : ControllerBase
{
    private readonly IUserService _userService = userService;
    private readonly ICustomerService _customerService = customerService;
    private readonly ICurrentUserProvider _currentUser = currentUser;

    /// <summary>
    /// Register an account for admin by superadmin
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    [HttpPost("onboarding/admin")]
    [Authorize(Policy = Constants.PlatformRootAccessPolicy)]
    [EndpointDescription("This endpoint will act the registration endpoint for admin, action is done by SuperAdmin only")]
    public async Task<ActionResult> Register([FromBody] OnboardingUserDto dto)
    {
        var (userId, token) = await _userService.OnboardingUserAsync(dto);
        return Ok( new
        {
            UserId = userId,
            Token = token,
        });
    }

    /// <summary>
    /// Global login endpoint for all users, no matter what role
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult> Login(
        [FromBody] LoginDto dto)
    {
        var result = await _userService.LoginAsync(dto);

        return result is null 
            ? BadRequest("Invalid credentials!")
            : Ok(result);
    }
    
    /// <summary>
    /// This will act as the register route for a customer
    /// </summary>
    /// <param name="dto"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost("register")]
    [EndpointDescription("This endpoint act a public endpoint for customer to register")]
    [AllowAnonymous] 
    public async Task<ActionResult> CreateCustomerAsync(
        [FromBody] CustomerCreateDto dto,
        CancellationToken cancellationToken = default)
    {
        var customer = await _customerService.CreateAsync(dto, cancellationToken);
        return Ok(new
        {
            Message = "Register successfully!",
            dto.UserName,
            customer.Email
        });
    }
    
    /// <summary>
    /// Complete invitation for tenant (generated token)
    /// </summary>
    /// <param name="req"></param>
    /// <returns></returns>
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

    /// <summary>
    /// Get a profile for authenticated user, TODO: plan for the profile, whether using static structure or using switch to map: user, tenant, or superadmin
    /// </summary>
    /// <returns></returns>
    [HttpGet("me")]
    [Authorize(Policy = Constants.RequireAuthenticatedUser)]
    public ActionResult GetMe()
    {
        if (!_currentUser.IsAuthenticated)
            return Unauthorized();
         
        return Ok(new { _currentUser.UserId, _currentUser.Email, _currentUser.TenantId, _currentUser.Roles });
    }
    
    /// <summary>
    /// Change the current authenticated user's password
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("change-password")]
    [Authorize(Policy = Constants.RequireAuthenticatedUser)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        if (!_currentUser.IsAuthenticated)
            return Unauthorized();

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _userService.ChangePasswordAsync(_currentUser.UserId!, request);

        return result.Succeeded
            ? Ok(new { message = "Changed successfully." })
            : BadRequest(new { message = "Failed to change password. Please check your current password." });
    }
}
