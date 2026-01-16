using Microsoft.EntityFrameworkCore.Storage;

namespace CoreAPI.Repositories.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IUserRepository UserRepository { get; }
    ICustomerRepository CustomerRepository { get; }
    ITenantRepository TenantRepository { get; }
    IAccountRepository AccountRepository { get; }
    ITransactionRepository TransactionRepository { get; }
    ITransactionTypeRepository TransactionTypeRepository { get; }
    IAccountTypeRepository AccountTypeRepository { get; }

    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
    IRepository<T> GetRepository<T>() where T : class;
    Task<int> CompleteAsync(CancellationToken cancellationToken = default);
}