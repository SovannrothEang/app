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

    public async Task<IdentityResult> RegisterAsync(RegisterDto dto)
    {
        try
        {
            _logger.LogInformation("[AuthService] > Register a user");
            var emailIsExist = await _userManager.FindByEmailAsync(dto.Email);
            if (emailIsExist != null)
                throw new BadHttpRequestException("Email is already exist!");

            var userNameIsExist = await _userManager.FindByNameAsync(dto.UserName);
            if (userNameIsExist != null)
                throw new BadHttpRequestException("UserName is already exist!");

            var user = _mapper.Map<RegisterDto, User>(dto);
            var result = await _userManager.CreateAsync(user, dto.Password);
            //if (!result.Succeeded) return false;

            //await SendEmailVerificationEmailAsync(user);
            return result;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            throw;
        }
    }

    public async Task<AuthResponseDto?> LoginAsync(LoginDto dto, CancellationToken ct = default)
    {
        try
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
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            throw;
        }
    }

    public async Task<UserProfileDto> CreateUserAsync(RegisterDto dto,
        CancellationToken ct = default)
    {
        try
        {
            _currentUserProvider.SetTenantId(_tenantHost);
            var emailIsExist = await _userManager.FindByEmailAsync(dto.Email);
            if (emailIsExist != null)
                throw new BadHttpRequestException("Email is already exist!");

            var userNameIsExist = await _userManager.FindByNameAsync(dto.UserName);
            if (userNameIsExist != null)
                throw new BadHttpRequestException("UserName is already exist!");

            var user = _mapper.Map<RegisterDto, User>(dto);
            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
                throw new BadHttpRequestException(string.Join(",\n",
                    result.Errors.Select(e => e.Description)));

            if (!await _roleManager.RoleExistsAsync("Customer"))
            {
                var role = new Role(Guid.NewGuid().ToString(), "Customer", _tenantHost);
                await _roleManager.CreateAsync(role);
            }

            await _userManager.AddToRoleAsync(user, "Customer");

            //await SendEmailVerificationEmailAsync(user);
            return _mapper.Map<UserProfileDto>(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            throw;
        }
    }
    
    public async Task<(string userId, string token)> CreateTenantAndUserAsync(TenantCreateDto dto, CancellationToken ct = default)
    {
        await _unitOfWork.BeginTransactionAsync(ct);
        try
        {
            // Tenant Creation
            var tenant = _mapper.Map<Tenant>(dto.Tenant);
            await _tenantRepository.CreateAsync(tenant, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            // Role Creation
            const string roleName = "TenantOwner";
            var role = new Role(Guid.NewGuid().ToString(), roleName, tenant.Id);
            var roleCreated = await _roleManager.CreateAsync(role);
            if (!roleCreated.Succeeded)
                throw new Exception(roleCreated.Errors.First().Description);
            
            // User Creation
            var user = new User(Guid.NewGuid().ToString(), dto.Owner.Email, dto.Owner.UserName, tenant.Id)
            {
                EmailConfirmed = true
            };
            var result = await _userManager.CreateAsync(user);
            if (!result.Succeeded)
                throw new Exception(result.Errors.First().Description);
            
            // Assign role to user
            await _userRepository.AddToRoleAsync(user.Id, role.Id);

            await _unitOfWork.SaveChangesAsync(ct);
            await _unitOfWork.CommitAsync(ct);
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            
            // TODO: use EmailService, and send the invited-link to the tenant's email instead
            return (user.Id, token); // Development
        }
        catch
        {
            await _unitOfWork.RollbackAsync(ct);
            throw;
        }
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
            var roles = await _userManager.GetRolesAsync(user);
            var dto = _mapper.Map<UserProfileDto>(user);
            result.Add(dto with { Roles = roles });
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