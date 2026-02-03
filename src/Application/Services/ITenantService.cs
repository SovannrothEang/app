using Application.DTOs.Tenants;
using Domain.Shared;

namespace Application.Services;

public interface ITenantService
{
    Task<PagedResult<TenantDto>> GetPagedResultsAsync(PaginationOption option, CancellationToken cancellationToken = default);
    Task<TenantDto> GetByIdAsync(string id, CancellationToken ct = default);
    Task<TenantDto> GetValidTenantByIdAsync(string id, bool trackChange = false, CancellationToken ct = default);
    Task<TenantOnboardResponseDto> CreateAsync(TenantOnBoardingDto dto, CancellationToken ct = default);
    Task UpdateAsync(string id, TenantUpdateDto dto, CancellationToken ct = default);
    Task DeleteAsync(string id, CancellationToken ct = default);

    Task ActivateAsync(string id, CancellationToken ct = default);
    Task DeactivateAsync(string id, CancellationToken ct = default);
}