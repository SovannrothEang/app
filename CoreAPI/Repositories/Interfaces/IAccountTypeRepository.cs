using CoreAPI.Models;

namespace CoreAPI.Repositories.Interfaces;

public interface IAccountTypeRepository
{
    Task<IEnumerable<AccountType>> GetAllAsync(bool childIncluded, CancellationToken cancellationToken);
    Task<AccountType?> GetByIdAsync(string id, bool childIncluded, CancellationToken cancellationToken);
    Task<bool> ExistsAsync(string id, CancellationToken cancellationToken = default);
    
    Task CreateAccountTypeAsync(AccountType newAccountType, CancellationToken cancellationToken = default);
}