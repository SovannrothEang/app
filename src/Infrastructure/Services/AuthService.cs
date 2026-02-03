using Application;
using Application.DTOs;
using Application.DTOs.Auth;
using Application.DTOs.Tenants;
using Application.Services;
using Application.Validators.Auth;
using AutoMapper;
using Domain.Entities;
using Domain.Repositories;
using Domain.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

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
    private readonly IRepository<UserRole> _userRoleRepository = unitOfWork.GetRepository<UserRole>();
    private readonly IRepository<Role> _roleRepository = unitOfWork.GetRepository<Role>();
    private readonly IMapper _mapper = mapper;
    //private readonly IEmailSender<User> _emailSender = emailSender;
    private readonly ITokenService _tokenService = tokenService;
    private readonly ICurrentUserProvider _currentUserProvider = currentUserProvider;
    private readonly ILogger<AuthService> _logger = logger;
    #endregion

    private async Task CheckEmailAndUserNameExistence(string email, string userName, CancellationToken ct = default)
    {
        var emailExist = await _repository.ExistsAsync(
            predicate: e => e.Email == email,
            ignoreQueryFilters: true, // Checking across all tenants
            cancellationToken: ct);
        if (emailExist)
        {
            if (_logger.IsEnabled(LogLevel.Warning))
                _logger.LogWarning("Email already exists: {Email}", email);
            throw new BadHttpRequestException("This credential is already exist!");
        }
        var userNameExist = await _repository.ExistsAsync(
            predicate: e => e.UserName == userName,
            ignoreQueryFilters: true, // Checking across all tenants
            cancellationToken: ct);
        if (userNameExist)
        {
            if (_logger.IsEnabled(LogLevel.Warning))
                _logger.LogWarning("UserName already exists: {UserName}", userName);
            throw new BadHttpRequestException("This credential is already exist!");
        }
    }
    
    public async Task<AuthResponseDto?> LoginAsync(LoginDto dto, CancellationToken ct = default)
    {
        if (_logger.IsEnabled(LogLevel.Information))
            _logger.LogInformation("User login attempt: {userName}", dto.UserName);
        var user = await _userManager.Users
            // Ignore filtering for search for a user
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.NormalizedUserName == dto.UserName.ToUpper(), ct);
        if (user == null)
        {
            if (_logger.IsEnabled(LogLevel.Warning))
                _logger.LogWarning("Login failed. User not found: {userName}", dto.UserName);
            return null;
        }

        var signinResult = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, lockoutOnFailure: false);
        if (!signinResult.Succeeded)
        {
            if (_logger.IsEnabled(LogLevel.Warning))
                _logger.LogWarning("Login failed. Invalid credentials for user: {UserName}", dto.UserName);
            return null;
        }

        var (token, expiresAt) = await _tokenService.GenerateToken(user);
        var roles = await _unitOfWork.GetRepository<UserRole>().ListAsync(
            filter: u => u.UserId == user.Id && u.TenantId == user.TenantId,
            ignoreQueryFilters: true,
            includes: e => e.Include(u => u.Role),
            select: u => u.Role!.Name,
            cancellationToken: ct);
        
        return new AuthResponseDto(
            token,
            expiresAt,
            null!,
            user.Id,
            user.Email!,
            [.. roles!]
        );
    }

    public async Task<(string userId, string token)> OnboardingUserAsync(OnboardingUserDto dto, CancellationToken ct = default)
    {
        var userId = _currentUserProvider.UserId!;
        if (_logger.IsEnabled(LogLevel.Information))
            _logger.LogInformation("Onboarding user id: {UserId}, attempt by {PerformBy}", userId, _currentUserProvider.UserId);
        await using var transaction = await _unitOfWork.BeginTransactionAsync(ct);
        try
        {
            var roleName = dto.Role ?? RoleConstants.User;
            var roleExists = await _roleManager.RoleExistsAsync(roleName);
            if (!roleExists)
            {
                if (_logger.IsEnabled(LogLevel.Warning))
                    _logger.LogWarning("Onboarding failed. Role does not exist: {Role}", roleName);
                throw new BadHttpRequestException($"Role '{roleName}' does not exist!");
            }
            await CheckEmailAndUserNameExistence(dto.Email, dto.UserName, ct);
            var user = _mapper.Map<User>(dto);
            user.TenantId = _currentUserProvider.TenantId!;
            user.PerformBy = userId;
            var result = await _userManager.CreateAsync(user);
            if (!result.Succeeded)
            {
                if (_logger.IsEnabled(LogLevel.Warning))
                    _logger.LogWarning("Onboarding failed. User creation failed for: {UserName}", dto.UserName);
                throw new BadHttpRequestException(string.Join(", ", result.Errors.Select(e => e.Description)));
            }

            var roleCreated = await _userManager.AddToRoleAsync(user, roleName);
            if (!roleCreated.Succeeded)
            {
                if (_logger.IsEnabled(LogLevel.Warning))
                    _logger.LogWarning("Onboarding failed. Assigning role '{Role}' to user '{UserName}' failed", roleName, dto.UserName);
                throw new BadHttpRequestException(string.Join(", ", roleCreated.Errors.Select(error => error.Description)));
            }

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

    // Customer registration
    public async Task<UserProfileDto> CreateUserAsync(RegisterDto dto, CancellationToken ct = default)
    {
        if (_logger.IsEnabled(LogLevel.Information))
            _logger.LogInformation("Customer registration attempt: {UserName} and {Email}.", dto.UserName, dto.Email);
        await CheckEmailAndUserNameExistence(dto.Email, dto.UserName, ct);

        var user = _mapper.Map<RegisterDto, User>(dto);
        _currentUserProvider.SetTenantId(_tenantHost);
        user.TenantId = _tenantHost; // Global customer
        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
        {
            if (_logger.IsEnabled(LogLevel.Warning))
                _logger.LogWarning("Registration failed. User creation failed for: {UserName}", dto.UserName);
            throw new BadHttpRequestException(string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        if (!await _roleManager.RoleExistsAsync(RoleConstants.Customer))
        {
            var role = new Role(Guid.NewGuid().ToString(), RoleConstants.Customer, _tenantHost);
            var roleCreated = await _roleManager.CreateAsync(role);
            if (!roleCreated.Succeeded)
            {
                if (_logger.IsEnabled(LogLevel.Warning))
                    _logger.LogWarning("Registration failed. Creating default role '{role}' failed", RoleConstants.Customer);
                throw new BadHttpRequestException(string.Join(", ", roleCreated.Errors.Select(error => error.Description)));
            }
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
        if (_logger.IsEnabled(LogLevel.Information))
            _logger.LogInformation("Creating tenant owner user for tenant: {TenantName}", tenant.Name);
        await CheckEmailAndUserNameExistence(dto.Email, dto.UserName, ct);
        var user = new User(Guid.NewGuid().ToString(), dto.Email, dto.UserName, tenant.Id)
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            PerformBy = _currentUserProvider.UserId,
        };
        var result = await _userManager.CreateAsync(user);
        if (!result.Succeeded)
        {
            if (_logger.IsEnabled(LogLevel.Warning))
                _logger.LogWarning("Onboarding user for tenant failed. User creation failed for: {UserName}", dto.UserName);
            throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));
        }
        
        // Default roles
        var defaultRoles = new List<Role>
        {
            new(Guid.NewGuid().ToString(), RoleConstants.TenantOwner, tenant.Id)
            {
                NormalizedName = RoleConstants.TenantOwner.ToUpper(),
                PerformBy = _currentUserProvider.UserId
            },
            new(Guid.NewGuid().ToString(), RoleConstants.User, tenant.Id)
            {
                NormalizedName = RoleConstants.User.ToUpper(),
                PerformBy = _currentUserProvider.UserId
            }
        };
        await _roleRepository.CreateBatchAsync(defaultRoles, ct);

        // Assign role to user
        var newUserRole = new UserRole
        {
            UserId = user.Id,
            RoleId = defaultRoles[0].Id,
            TenantId = tenant.Id
        };
        await _userRoleRepository.CreateAsync(newUserRole, ct);

        await _unitOfWork.CompleteAsync(ct);
        var token = await _userManager.GeneratePasswordResetTokenAsync(user);

        // TODO: use EmailService, and send the invited-link to the tenant's email instead
        return (user.Id, token); // Development
    }

    public async Task<UserProfileDto> CompleteInviteAsync(string userId, string token, string newPassword)
    {
        if (_logger.IsEnabled(LogLevel.Information))
            _logger.LogInformation("Completing invite for user id: {UserId}", userId);
        var user = await _repository.FirstOrDefaultAsync(
            predicate: e => e.Id == userId,
            trackChanges: true,
            ignoreQueryFilters: true);
        if (user is null)
        {
            if (_logger.IsEnabled(LogLevel.Warning))
                _logger.LogWarning("Completing invite failed. User not found for id: {UserId}", userId);
            throw new Exception("User not found");
        }
        user.Modified();
        user.EmailConfirmed = true;
        await _unitOfWork.CompleteAsync();

        var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
        if (!result.Succeeded)
        {
            if (_logger.IsEnabled(LogLevel.Warning))
                _logger.LogWarning("Completing invite failed for user id: {userId}. Reason: {reason}", userId, string.Join(", ", result.Errors.Select(e => e.Description)));
            throw new BadHttpRequestException("Invalid token or password complexity failed.");
        }

        var roles = await _unitOfWork.GetRepository<UserRole>().ListAsync(
            filter: u => u.UserId == user.Id && u.TenantId == user.TenantId,
            ignoreQueryFilters: true,
            includes: e => e.Include(u => u.Role),
            select: u => u.Role!.Name);
        return new UserProfileDto(
            user.Id,
            user.UserName,
            user.Email,
            [.. roles!]);
    }

    #region Auth
    public async Task<UserProfileDto> GetCurrentUserProfileAsync(string userId)
    {
        var user = await _repository.FirstOrDefaultAsync(
            predicate: e => e.Id == userId,
            includes: q => q
                .Include(x => x.UserRoles!)
                .ThenInclude(x => x.Role));
        if (user is null)
        {
            if (_logger.IsEnabled(LogLevel.Warning))
                _logger.LogWarning("Unauthorized access attempt for user id: {UserId}", userId);
            throw new UnauthorizedAccessException();
        }
        return _mapper.Map<UserProfileDto>(user);
    }

    public async Task<IdentityResult> ChangePasswordAsync(string userId, ChangePasswordRequest request)
    {
        var result = await new ChangePasswordRequestValidator(_userManager, _currentUserProvider).ValidateAsync(request);
        if (!result.IsValid)
        {
            if (_logger.IsEnabled(LogLevel.Warning))
                _logger.LogWarning("Change password validation failed for user id: {userId}. Errors: {errors}", userId, result.Errors);
            throw new BadHttpRequestException(string.Join(", ", result.Errors.Select(e => e.ErrorMessage)));
        }
        
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

    public async Task<PagedResult<UserProfileDto>> GetAllUserAsync(
        PaginationOption option,
        CancellationToken ct = default)
    {
        option.Page ??= 1;
        option.PageSize ??= 10;
        var (users, totalCount) = await _repository.GetPagedResultAsync(
            option: option,
            ignoreQueryFilters: true,
            includes: q => q
                .Include(e => e.UserRoles!)
                .ThenInclude(ur => ur.Role),
            cancellationToken: ct);
        return new PagedResult<UserProfileDto>
        {
            Items = [.. users.Select(_mapper.Map<UserProfileDto>)],
            PageNumber = option.Page.Value,
            PageSize = option.PageSize.Value,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling((double)totalCount / option.PageSize.Value)
        };
    }

    public async Task<PagedResult<UserProfileDto>> GetPagedResultAsync(PaginationOption option, CancellationToken ct = default)
    {
        option.Page ??= 1;
        option.PageSize ??= 10;
        var (users, totalCount) = await _repository.GetPagedResultAsync(
            option: option,
            // ignoreQueryFilters: true,
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
        return new PagedResult<UserProfileDto>
        {
            Items = [.. users.Select(_mapper.Map<UserProfileDto>)],
            PageNumber = option.Page.Value,
            PageSize = option.PageSize.Value,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling((double)totalCount / option.PageSize.Value)
        };
    }

    public async Task<PagedResult<UserProfileDto>> GetPagedResultForAdminAsync(PaginationOption option, CancellationToken ct = default)
    {
        option.Page ??= 1;
        option.PageSize ??= 10;
        var (users, totalCount) = await _repository.GetPagedResultAsync(
            option: option,
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
        return new PagedResult<UserProfileDto>
        {
            Items = [.. users.Select(_mapper.Map<UserProfileDto>)],
            PageNumber = option.Page.Value,
            PageSize = option.PageSize.Value,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling((double)totalCount / option.PageSize.Value)
        };
    }

    public async Task<UserProfileDto?> GetUserByIdAsync(string id, CancellationToken ct = default)
    {
        var user = await _repository.FirstOrDefaultAsync(
                       e => e.Id == id,
                       includes: q => q
                           .Include(e => e.UserRoles!)
                           .ThenInclude(ur => ur.Role),
                       cancellationToken: ct)
                   ?? throw new KeyNotFoundException($"No User was found with id: {id}");
        
        return _mapper.Map<UserProfileDto>(user);
    }
    
    public async Task<UserProfileDto?> GetUserByIdForAdminAsync(string id, CancellationToken ct = default)
    {
        var user = await _repository.FirstOrDefaultAsync(
           e => e.Id == id,
           ignoreQueryFilters: true,
           includes: q => q
               .Include(e => e.UserRoles!)
               .ThenInclude(ur => ur.Role),
           cancellationToken: ct)
           ?? throw new KeyNotFoundException($"No User was found with id: {id}");
        
        return _mapper.Map<UserProfileDto>(user);
    }
}
