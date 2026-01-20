using CoreAPI.DTOs;
using CoreAPI.DTOs.Tenants;

namespace CoreAPI.Services.Interfaces;

public interface ITenantService
{
    Task<IEnumerable<TenantDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<PagedResult<TenantDto>> GetPagedResultsAsync(PaginationOption option, CancellationToken cancellationToken = default);
    Task<TenantDto?> GetByIdAsync(string id, CancellationToken ct = default);
    Task<TenantDto?> GetValidTenantByIdAsync(string id, bool trackChange = false, CancellationToken ct = default);
    Task<TenantOnboardResponseDto> CreateAsync(TenantOnBoardingDto dto, CancellationToken ct = default);
    Task UpdateAsync(string id, TenantUpdateDto dto, CancellationToken ct = default);
    Task DeleteAsync(string id, CancellationToken ct = default);

    Task ActivateAsync(string id, CancellationToken ct = default);
    Task DeactivateAsync(string id, CancellationToken ct = default);
}