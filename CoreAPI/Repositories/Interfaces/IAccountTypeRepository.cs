using CoreAPI.Models;

namespace CoreAPI.Repositories.Interfaces;

public interface IAccountTypeRepository
{
    Task<IEnumerable<AccountType>> GetAccountTypesAsync(CancellationToken cancellationToken);
    Task<AccountType?> GetAccountTypeAsync(string id, CancellationToken cancellationToken);
    Task<bool> ExistsAsync(string id, CancellationToken cancellationToken = default);
    
    Task CreateAccountTypeAsync(AccountType newAccountType, CancellationToken cancellationToken = default);
}