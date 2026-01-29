using CoreAPI.DTOs.Auth;
using CoreAPI.DTOs.Customers;
using CoreAPI.Services.Interfaces;
using CoreAPI.Validators.Auth;
using CoreAPI.Validators.Customers;
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
    ICurrentUserProvider currentUser,
    ILogger<AuthController> logger) : ControllerBase
{
    #region Private Fields
    private readonly IUserService _userService = userService;
    private readonly ICustomerService _customerService = customerService;
    private readonly ICurrentUserProvider _currentUser = currentUser;
    private readonly ILogger<AuthController> _logger = logger;
    #endregion

    /// <summary>
    /// Global login endpoint for all users, no matter what role
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult> Login(
        [FromBody] LoginDto dto)
    {
        var result = await _userService.LoginAsync(dto);

        if (result is null)
        {
            if (_logger.IsEnabled(LogLevel.Warning))
                _logger.LogWarning("[AuthController] Login failed for user {UserName}", dto.UserName);
            return BadRequest("Invalid credentials!");
        }

        return Ok(result);
    }
    
    /// <summary>
    /// This will act as the register route for a customer
    /// </summary>
    [HttpPost("register")]
    [EndpointDescription("This endpoint act a public endpoint for customer to register")]
    [AllowAnonymous] 
    public async Task<ActionResult> CreateCustomerAsync(
        [FromBody] CustomerCreateDto dto,
        CancellationToken cancellationToken = default)
    {
        var result = new CustomerCreateDtoValidator().Validate(dto);
        if (!result.IsValid)
        {
            if (_logger.IsEnabled(LogLevel.Warning))
                _logger.LogWarning("[AuthController] Customer registration validation failed: {Errors}", result.Errors);
            return BadRequest(result.Errors);
        }

        var customer = await _customerService.CreateAsync(dto, cancellationToken);
        return Ok(new
        {
            Message = "Register successfully!",
            dto.UserName,
            customer.Email
        });
    }
    
    /// <summary>
    /// Complete invitation for tenant
    /// </summary>
    [HttpPost("complete-invite")]
    [AllowAnonymous]
    public async Task<IActionResult> CompleteInvite([FromBody] SetupPasswordRequest req)
    {
        var result = new SetupPasswordRequestValidator().Validate(req);
        if (!result.IsValid)
        {
            if (_logger.IsEnabled(LogLevel.Warning))
                _logger.LogWarning("[AuthController] Complete invite validation failed: {Errors}", result.Errors);
            return BadRequest(result.Errors);
        }
        var user = await _userService.CompleteInviteAsync(req.UserId, req.Token, req.NewPassword);
        return Ok(user);
    }

    /// <summary>
    /// Get a profile for authenticated user,
    /// TODO: plan for the profile, whether using static structure or using switch to map: user, tenant, or superadmin
    /// </summary>
    [HttpGet("me")]
    [Authorize(Policy = Constants.RequireAuthenticatedUser)]
    public ActionResult GetMe()
    {
        if (!_currentUser.IsAuthenticated)
        {
            if (_logger.IsEnabled(LogLevel.Warning))
                _logger.LogWarning("[AuthController] Unauthorized access to GetMe endpoint.");
            return Unauthorized();
        }
            
         
        return Ok(new { _currentUser.UserId, _currentUser.Email, _currentUser.TenantId, _currentUser.Roles });
    }
    
    /// <summary>
    /// Change the current authenticated user's password
    /// </summary>
    [HttpPost("change-password")]
    [Authorize(Policy = Constants.RequireAuthenticatedUser)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        if (!_currentUser.IsAuthenticated)
        {
            if (_logger.IsEnabled(LogLevel.Warning))
                _logger.LogWarning("[AuthController] Unauthorized access to ChangePassword endpoint.");
            return Unauthorized();
        }

        // TODO: prepare for using fluent validation instead
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _userService.ChangePasswordAsync(_currentUser.UserId!, request);

        if (!result.Succeeded)
        {
            if (_logger.IsEnabled(LogLevel.Warning))
                _logger.LogWarning("[AuthController] Change password failed for user {UserId}: {Errors}", _currentUser.UserId, result.Errors);
            return BadRequest(new { message = "Failed to change password. Please check your current password." });
        }

        return Ok(new { message = "Changed successfully." });
    }
}
