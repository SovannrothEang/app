using AutoMapper;
using CoreAPI.DTOs;
using CoreAPI.DTOs.Auth;
using CoreAPI.DTOs.Tenants;
using CoreAPI.Models;
using CoreAPI.Repositories.Interfaces;
using CoreAPI.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CoreAPI.Services;

public class AuthService(
    UserManager<User> userManager,
    RoleManager<Role> roleManager,
    SignInManager<User> signInManager,
    IConfiguration configuration,
    IUnitOfWork unitOfWork,
    IMapper mapper,
    ITokenService tokenService,
    ICurrentUserProvider currentUserProvider,
    ILogger<AuthService> logger) : IUserService
{
    private readonly UserManager<User> _userManager = userManager;
    private readonly RoleManager<Role> _roleManager = roleManager;
    private readonly SignInManager<User> _signInManager = signInManager;
    private readonly string _tenantHost = configuration["Tenancy:Host"]
                                          ?? throw new Exception("Tenant host not found");
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IUserRepository _userRepository = unitOfWork.UserRepository;
    private readonly ITenantRepository _tenantRepository = unitOfWork.TenantRepository;
    private readonly IMapper _mapper = mapper;

    //private readonly IEmailSender<User> _emailSender = emailSender;
    private readonly ITokenService _tokenService = tokenService;
    private readonly ICurrentUserProvider _currentUserProvider = currentUserProvider;
    private readonly ILogger<AuthService> _logger = logger;

    public async Task<(string userId, string token)> OnboardingUserAsync(OnboardingUserDto dto, CancellationToken ct = default)
    {
        var userId = _currentUserProvider.UserId!;
        if (_logger.IsEnabled(LogLevel.Information))
            _logger.LogInformation("Onboarding user by admin: {userId}", userId);
        await using var transaction = await _unitOfWork.BeginTransactionAsync(ct);
        try
        {
            var emailIsExist = await _userManager.FindByEmailAsync(dto.Email);
            if (emailIsExist != null)
                throw new BadHttpRequestException("Email is already exist!");

            var userNameIsExist = await _userManager.FindByNameAsync(dto.UserName);
            if (userNameIsExist != null)
                throw new BadHttpRequestException("UserName is already exist!");

            var user = _mapper.Map<OnboardingUserDto, User>(dto);
            user.TenantId = _tenantHost;
            var result = await _userManager.CreateAsync(user);
            //if (!result.Succeeded) return false;

            var roleCreated = dto.Role is null
                ? await _userManager.AddToRoleAsync(user, RoleConstants.SuperAdmin)
                : await _userManager.AddToRoleAsync(user, dto.Role);
            if (!roleCreated.Succeeded)
                throw new BadHttpRequestException(string.Join(", ",
                    roleCreated.Errors.Select(error => error.Description)));

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            //await SendEmailVerificationEmailAsync(user);
            await _unitOfWork.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
            return (user.Id, token);
        }
        catch 
        {
            await transaction.RollbackAsync(ct);
            throw;
        }
    }

    public async Task<AuthResponseDto?> LoginAsync(LoginDto dto, CancellationToken ct = default)
    {
        var user = await _userManager.Users
            // Ignore filtering for search for a user
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.NormalizedUserName == dto.UserName.ToUpper(), ct);
        if (user == null) return null;

        var signinResult = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, lockoutOnFailure: false);
        if (!signinResult.Succeeded) return null;

        var (token, expiresAt) = await _tokenService.GenerateToken(user);
        var roles = await _userManager.GetRolesAsync(user);

        return new AuthResponseDto(
            token,
            expiresAt,
            null!,
            user.Id,
            user.Email!,
            roles
        );
    }

    public async Task<UserProfileDto> CreateUserAsync(RegisterDto dto,
        CancellationToken ct = default)
    {
        _currentUserProvider.SetTenantId(_tenantHost);
        var emailIsExist = await _userManager.FindByEmailAsync(dto.Email);
        if (emailIsExist != null)
            throw new BadHttpRequestException("Email is already exist!");

        var userNameIsExist = await _userManager.FindByNameAsync(dto.UserName);
        if (userNameIsExist != null)
            throw new BadHttpRequestException("UserName is already exist!");

        var user = _mapper.Map<RegisterDto, User>(dto);
        user.TenantId = _tenantHost; // Global customer
        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
            throw new BadHttpRequestException(string.Join(",\n",
                result.Errors.Select(e => e.Description)));

        if (!await _roleManager.RoleExistsAsync(RoleConstants.Customer))
        {
            var role = new Role(Guid.NewGuid().ToString(), RoleConstants.Customer, _tenantHost);
            await _roleManager.CreateAsync(role);
        }

        await _userManager.AddToRoleAsync(user, RoleConstants.Customer);

        //await SendEmailVerificationEmailAsync(user);
        return _mapper.Map<UserProfileDto>(user);
    }
    
    public async Task<(string userId, string token)> CreateTenantAndUserAsync(TenantCreateDto dto, CancellationToken ct = default)
    {
        await using var transaction = await _unitOfWork.BeginTransactionAsync(ct);
        try
        {
            // Tenant Creation
            var tenant = _mapper.Map<Tenant>(dto.Tenant);
            tenant.AddPerformBy(_currentUserProvider.UserId);
            await _tenantRepository.CreateAsync(tenant, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            // User Creation
            var user = new User(Guid.NewGuid().ToString(), dto.Owner.Email, dto.Owner.UserName, tenant.Id)
            {
                EmailConfirmed = true
            };
            var result = await _userManager.CreateAsync(user);
            if (!result.Succeeded)
                throw new Exception(result.Errors.First().Description);
            
            // Make sure role exist
            var role = await EnsuringRoleExistsAsync(RoleConstants.TenantOwner, tenant.Id);
            
            // Assign role to user
            await _userRepository.AddToRoleAsync(user.Id, role.Id);

            await _unitOfWork.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            
            // TODO: use EmailService, and send the invited-link to the tenant's email instead
            return (user.Id, token); // Development
        }
        catch
        {
            await transaction.RollbackAsync(ct);
            throw;
        }
    }

    private async Task<Role> EnsuringRoleExistsAsync(string roleName, string tenantId)
    {
        var role = await _roleManager.Roles.FirstOrDefaultAsync(r => r.Name == roleName);
        if (role != null)
        {
            return role;
        }

        var newRole = new Role(Guid.NewGuid().ToString(), roleName, tenantId);
        var result = await _roleManager.CreateAsync(newRole);
        return result.Succeeded
            ? newRole
            : throw new BadHttpRequestException(
                string.Join(", ", result.Errors.Select(e => e.Description)));
    }
    
    public async Task CompleteInviteAsync(string userId, string token, string newPassword)
    {
        var user = await _userManager.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null) throw new Exception("User not found");

        var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
        
        if (!result.Succeeded) 
            throw new Exception("Invalid token or password complexity failed.");
    }

    #region Auth
    public async Task<UserProfileDto> GetCurrentUserProfileAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId)
                   ?? throw new UnauthorizedAccessException("User not found.");

        var roles = await _userManager.GetRolesAsync(user);
        return new UserProfileDto(user.Id, user.UserName!, user.Email!, roles);
    }

    public async Task<IdentityResult> ChangePasswordAsync(string userId, ChangePasswordRequest request)
    {
        var user = await _userManager.FindByIdAsync(userId);
        return await _userManager.ChangePasswordAsync(user!, request.CurrentPassword, request.NewPassword);
    }

    //public Task SendPasswordResetEmailAsync(string email)
    //{
    //    throw new NotImplementedException();
    //}

    public Task<IdentityResult> ResetPasswordAsync(string email, string token, string newPassword)
    {
        throw new NotImplementedException();
    }
    
    #endregion

    public async Task<IEnumerable<UserProfileDto>> GetAllUserAsync(CancellationToken ct = default)
    {
        var users = await _userRepository.GetAllAsync(cancellationToken: ct);
        var result = new List<UserProfileDto>();

        foreach (var user in users)
        {
            var roles = await _userRepository.GetAllRolesAsync(user.Id, ct);
            var dto = _mapper.Map<UserProfileDto>(user);
            result.Add(dto with { Roles = roles.ToList() });
        }
        return result;
    }

    public async Task<UserProfileDto?> GetUserById(string id, CancellationToken ct = default)
    {
        var user = await _userRepository.GetByIdAsync(id, ct)
                   ?? throw new KeyNotFoundException($"No User was found with id: {id}");
        var roles = await _userManager.GetRolesAsync(user);
        var dto = _mapper.Map<UserProfileDto>(roles);
        return dto with { Roles = roles };
    }
}