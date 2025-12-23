using CoreAPI.DTOs.Tenants;

namespace CoreAPI.Services.Interfaces;

public interface ITenantService
{
    Task<IEnumerable<TenantDto>> GetAllAsync(CancellationToken ct = default);
    Task<IEnumerable<TenantDto>> GetAllWithIncludesAsync(CancellationToken ct = default);
    Task<TenantDto?> GetByIdAsync(string id, CancellationToken ct = default);

    Task ActivateAsync(string id, CancellationToken ct = default);
    Task DeactivateAsync(string id, CancellationToken ct = default);
}