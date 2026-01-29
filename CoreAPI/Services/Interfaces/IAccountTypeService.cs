using CoreAPI.DTOs.Accounts;

namespace CoreAPI.Services.Interfaces;

public interface IAccountTypeService
{
    Task<IEnumerable<AccountTypeDto>> GetAllAsync(CancellationToken ct = default);
    Task<AccountTypeDto> GetByIdAsync(string id, CancellationToken ct = default);
    Task<AccountTypeDto> CreateAsync(AccountTypeCreateDto dto, CancellationToken ct = default);
    Task<bool> ExistsAsync(string id, CancellationToken ct = default);
}
