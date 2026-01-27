using CoreAPI.DTOs.Auth;
using CoreAPI.Models;
using Microsoft.AspNetCore.Identity;

namespace CoreAPI.Services.Interfaces;

public interface IRoleService
{
    IEnumerable<RoleDto> GetAllRoles(CancellationToken ct = default);
    Task<IEnumerable<string>> GetRoleNamesAsync(User user);
    Task<RoleDto?> GetRoleByIdAsync(string id);
    Task<bool> ExistsAsync(string roleName, CancellationToken ct = default);
    Task<IdentityResult> CreateRoleAsync(RoleCreateDto dto);
    Task<IdentityResult> AssignRoleAsync(AssignRoleDto dto);
}
