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
    # region Private Fields
    private readonly UserManager<User> _userManager = userManager;
    private readonly RoleManager<Role> _roleManager = roleManager;
    private readonly SignInManager<User> _signInManager = signInManager;
    private readonly string _tenantHost = configuration["Tenancy:Host"] ?? throw new Exception("Tenant host not found");
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IRepository<User> _repository = unitOfWork.GetRepository<User>();
    private readonly IUserRepository _userRepository = unitOfWork.UserRepository;
    private readonly IMapper _mapper = mapper;
    //private readonly IEmailSender<User> _emailSender = emailSender;
    private readonly ITokenService _tokenService = tokenService;
    private readonly ICurrentUserProvider _currentUserProvider = currentUserProvider;
    private readonly ILogger<AuthService> _logger = logger;
    #endregion

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
            await _userManager.CreateAsync(user);
            //if (!result.Succeeded) return false;

            var roleCreated = dto.Role is null
                ? await _userManager.AddToRoleAsync(user, RoleConstants.SuperAdmin)
                : await _userManager.AddToRoleAsync(user, dto.Role);
            if (!roleCreated.Succeeded)
                throw new BadHttpRequestException(string.Join(", ",
                    roleCreated.Errors.Select(error => error.Description)));

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            //await SendEmailVerificationEmailAsync(user);
            await _unitOfWork.CompleteAsync(ct);
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

    public async Task<UserProfileDto> CreateUserAsync(RegisterDto dto, CancellationToken ct = default)
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
            throw new BadHttpRequestException(string.Join(", ",
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

    public async Task<(string, string)> CreateTenantUserAsync(
        TenantDto tenant,
        TenantOwnerCreateDto dto,
        CancellationToken ct = default)
    {
        var user = new User(Guid.NewGuid().ToString(), dto.Email, dto.UserName, tenant.Id)
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
        };
        var result = await _userManager.CreateAsync(user);
        if (!result.Succeeded)
            throw new Exception(result.Errors.First().Description);

        // Make sure role exist
        var role = await EnsuringRoleExistsAsync(RoleConstants.TenantOwner, tenant.Id);

        // Assign role to user
        await _userRepository.AddToRoleAsync(user.Id, role.Id, tenant.Id);

        await _unitOfWork.CompleteAsync(ct);
        var token = await _userManager.GeneratePasswordResetTokenAsync(user);

        // TODO: use EmailService, and send the invited-link to the tenant's email instead
        return (user.Id, token); // Development
    }

    private async Task<Role> EnsuringRoleExistsAsync(string roleName, string tenantId)
    {
        var role = await _roleManager.Roles.FirstOrDefaultAsync(r => r.Name == roleName && r.TenantId == tenantId);
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

    public async Task<UserProfileDto> CompleteInviteAsync(string userId, string token, string newPassword)
    {
        var user = await _repository.FirstOrDefaultAsync(
            predicate: e => e.Id == userId,
            trackChanges: true,
            ignoreQueryFilters: true)
            ?? throw new Exception("User not found");
        user.Modified();
        user.EmailConfirmed = true;
        await _unitOfWork.CompleteAsync();

        var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
        if (!result.Succeeded)
            throw new BadHttpRequestException("Invalid token or password complexity failed.");

        var roles = await _userManager.GetRolesAsync(user);
        return new UserProfileDto(
            user.Id,
            user.UserName,
            user.Email,
            roles);
    }

    #region Auth
    public async Task<UserProfileDto> GetCurrentUserProfileAsync(string userId)
    {
        var user = await _repository.FirstOrDefaultAsync<UserProfileDto>(
            predicate: e => e.Id == userId,
            includes: q => q
                .Include(x => x.UserRoles!)
                .ThenInclude(x => x.Role))
            ?? throw new UnauthorizedAccessException();

        return user;
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
        var users = await _repository.ListAsync<UserProfileDto>(
            ignoreQueryFilters: true,
            includes: q => q
                .Include(e => e.UserRoles!)
                .ThenInclude(ur => ur.Role),
            cancellationToken: ct);
        return users;
    }

    public async Task<PagedResult<UserProfileDto>> GetPagedResultAsync(PaginationOption option, CancellationToken ct = default)
    {
        var users = await _repository.GetPagedAsync<UserProfileDto>(
            page: option.Page!.Value,
            pageSize: option.PageSize!.Value,
            ignoreQueryFilters: true,
            filter: option.FilterBy is null
                ? null
                : option.FilterBy.ToLower() switch
            {
                "username" => e => e.UserName == option.FilterValue,
                "email" => e => e.Email == option.FilterValue,
                "role" => e => e.UserRoles!.Any(x => x.Role!.Name == option.FilterValue),
                _ => null
            },
            includes: q => q
                .Include(e => e.UserRoles!)
                .ThenInclude(ur => ur.Role),
            cancellationToken: ct);
        return users;
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