using AutoMapper;
using CoreAPI.DTOs.Auth;
using CoreAPI.Models;
using CoreAPI.Repositories.Interfaces;
using CoreAPI.Services.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace CoreAPI.Services;

public class RoleService(
    UserManager<User> userManager,
    RoleManager<Role> roleManager,
    ICurrentUserProvider currentUserProvider,
    IUnitOfWork unitOfWork,
    IMapper mapper,
    ILogger<RoleService> logger) : IRoleService
{
    private readonly UserManager<User> _userManager = userManager;
    private readonly RoleManager<Role> _roleManager = roleManager;
    private readonly ICurrentUserProvider _currentUserProvider = currentUserProvider;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;
    private readonly ILogger<RoleService> _logger = logger;

    public IEnumerable<RoleDto> GetAllRoles(CancellationToken ct = default)
    {
        //var roles = await _roleManager.Roles.ToListAsync();
        var roles = _roleManager.Roles.ToList();
        return roles.Select(r => _mapper.Map<Role, RoleDto>(r));
    }

    public async Task<IEnumerable<string>> GetRoleNamesAsync(User user)
    {
        return await _userManager.GetRolesAsync(user);
    }

    public async Task<RoleDto?> GetRoleByIdAsync(string id)
    {
        var role = await _roleManager.FindByIdAsync(id);
        return role is null ? null : _mapper.Map<Role, RoleDto>(role);
    }
    
    public async Task<bool> ExistsAsync(string roleName, CancellationToken ct = default)
    {
        // var role = await _roleManager.FindByNameAsync(roleName);
        return await _unitOfWork.GetRepository<Role>().
            ExistsAsync(
                predicate: r => r.Name == roleName,
                cancellationToken: ct);
    }

    public async Task<IdentityResult> CreateRoleAsync(RoleCreateDto roleCreate)
    {
        var role = _mapper.Map<RoleCreateDto, Role>(roleCreate);
        role.PerformBy = _currentUserProvider.UserId;
        role.TenantId = _currentUserProvider.TenantId!;
        return await _roleManager.CreateAsync(role);
    }

    public async Task<IdentityResult> AssignRoleAsync(AssignRoleDto dto)
    {
        var user = await _userManager.FindByNameAsync(dto.UserName);
        if (user is null)
        {
            if (_logger.IsEnabled(LogLevel.Warning))
                _logger.LogWarning("No User is found with UserName: {UserName}!", dto.UserName);
            throw new KeyNotFoundException($"No User is found with UserName: {dto.UserName}!");
        }
        var role = await _roleManager.FindByNameAsync(dto.RoleName);
        if (role is null)
        {
            if (_logger.IsEnabled(LogLevel.Warning))
                _logger.LogWarning("No Role is found with RoleName: {RoleName}!", dto.RoleName);
            throw new KeyNotFoundException($"No Role is found with UserName: {dto.UserName}!");
        }
        if (await _userManager.IsInRoleAsync(user, dto.RoleName))
            throw new BadHttpRequestException($"User is already assigned to role: '{dto.RoleName}'!");

        return await _userManager.AddToRoleAsync(user, role.Name!);
    }
}
