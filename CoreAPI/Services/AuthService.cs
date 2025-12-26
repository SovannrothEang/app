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
    SignInManager<User> signInManager,
    IUnitOfWork unitOfWork,
    IMapper mapper,
    ITokenService tokenService) : IUserService
{
    private readonly UserManager<User> _userManager = userManager;
    private readonly SignInManager<User> _signInManager = signInManager;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IUserRepository _userRepository = unitOfWork.UserRepository;
    private readonly ITenantRepository _tenantRepository = unitOfWork.TenantRepository;
    private readonly IMapper _mapper = mapper;

    //private readonly IEmailSender<User> _emailSender = emailSender;
    private readonly ITokenService _tokenService = tokenService;

    public async Task<IdentityResult> RegisterAsync(RegisterDto dto)
    {
        var exist = await _userManager.FindByEmailAsync(dto.Email);
        if (exist != null) throw new BadHttpRequestException("Email is already exist!");

        var user = _mapper.Map<RegisterDto, User>(dto);
        var result = await _userManager.CreateAsync(user, dto.Password);
        //if (!result.Succeeded) return false;

        //await SendEmailVerificationEmailAsync(user);
        return result;
    }

    public async Task<AuthResponseDto?> LoginAsync(LoginDto dto, CancellationToken ct = default)
    {
        var user = await _userManager.Users
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
    
    public async Task<(string userId, string token)> CreateTenantAndUserAsync(TenantCreateDto dto, CancellationToken ct = default)
    {
        await _unitOfWork.BeginTransactionAsync(ct);
        try
        {
            var tenant = _mapper.Map<Tenant>(dto.Tenant);
            await _tenantRepository.CreateAsync(tenant, ct);
            // await _unitOfWork.SaveChangesAsync(ct);

            var user = new User(Guid.NewGuid().ToString(), dto.Owner.Email, dto.Owner.UserName, tenant.Id)
            {
                EmailConfirmed = true
            };
            var result = await _userManager.CreateAsync(user);
            if (!result.Succeeded)
                throw new Exception(result.Errors.First().Description);

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
    public async Task<UserProfileResponseDto> GetCurrentUserProfileAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId)
            ?? throw new UnauthorizedAccessException("User not found.");

        var roles = await _userManager.GetRolesAsync(user);
        return new UserProfileResponseDto(user.Id, user.UserName!, user.Email!, roles);
    }

    public async Task<IdentityResult> ChangePasswordAsync(string userId, ChangePasswordCommand command)
    {
        var user = await _userManager.FindByIdAsync(userId);
        return await _userManager.ChangePasswordAsync(user!, command.CurrentPassword, command.NewPassword);
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

    public async Task<IEnumerable<UserProfileResponseDto>> GetAllUserAsync(CancellationToken ct = default)
    {
        var users = await _userRepository.GetAllAsync(cancellationToken: ct);
        var result = new List<UserProfileResponseDto>();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var dto = _mapper.Map<UserProfileResponseDto>(user);
            result.Add(dto with { Roles = roles });
        }
        return result;
    }

    public async Task<UserProfileResponseDto?> GetUserById(string id, CancellationToken ct = default)
    {
        var user = await _userRepository.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"No User was found with id: {id}");
        var roles = await _userManager.GetRolesAsync(user);
        var dto = _mapper.Map<UserProfileResponseDto>(roles);
        return dto with { Roles = roles };
    }
}
