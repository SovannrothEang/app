using Application.DTOs.Auth;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Application.Services;

public interface IRoleService
{
    IEnumerable<RoleDto> GetAllRoles(CancellationToken ct = default);
    Task<IEnumerable<string>> GetRoleNamesAsync(User user);
    Task<RoleDto?> GetRoleByIdAsync(string id);
    Task<bool> ExistsAsync(string roleName, CancellationToken ct = default);
    Task<IdentityResult> CreateRoleAsync(RoleCreateDto dto);
    Task<IdentityResult> AssignRoleAsync(AssignRoleDto dto);
}
