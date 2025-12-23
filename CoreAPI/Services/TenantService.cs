using CoreAPI.DTOs.Tenants;
using CoreAPI.Repositories.Interfaces;
using CoreAPI.Services.Interfaces;

namespace CoreAPI.Services;

public class TenantService(ITenantRepository tenantRepository) : ITenantService
{
    private readonly ITenantRepository _tenantRepository = tenantRepository;

    public Task<IEnumerable<TenantDto>> GetAllAsync(CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<TenantDto>> GetAllWithIncludesAsync(CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task<TenantDto?> GetByIdAsync(string id, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public async Task ActivateAsync(string id, CancellationToken ct = default)
    {
        var tenant = await _tenantRepository.GetByIdAsync(id, ct)
                     ?? throw new KeyNotFoundException($"No tenant was found with id: {id}");

        tenant.Activate();
        await _tenantRepository.SaveChangeAsync(ct);
    }
    
    public async Task DeactivateAsync(string id, CancellationToken ct = default)
    {
        var tenant = await _tenantRepository.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"No tenant was found with id: {id}");
        
        tenant.Deactivate();
        await _tenantRepository.SaveChangeAsync(ct);
    }
}