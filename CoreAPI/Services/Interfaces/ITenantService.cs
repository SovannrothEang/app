using System.Linq.Expressions;
using CoreAPI.DTOs.Tenants;
using CoreAPI.Models;

namespace CoreAPI.Services.Interfaces;

public interface ITenantService
{
    Task<IEnumerable<TenantDto>> GetAllAsync(Expression<Func<Tenant, bool>>? filtering = null, CancellationToken cancellationToken = default);
    Task<TenantDto?> GetByIdAsync(string id, CancellationToken ct = default);
    
    Task<TenantDto> CreateAsync(TenantCreateDto dto, CancellationToken ct = default);
    Task UpdateAsync(string id, TenantUpdateDto dto, CancellationToken ct = default);
    Task DeleteAsync(string id, CancellationToken ct = default);

    Task ActivateAsync(string id, CancellationToken ct = default);
    Task DeactivateAsync(string id, CancellationToken ct = default);
}