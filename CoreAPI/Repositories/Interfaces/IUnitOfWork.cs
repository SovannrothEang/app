using Microsoft.EntityFrameworkCore.Storage;

namespace CoreAPI.Repositories.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IUserRepository UserRepository { get; }
    ICustomerRepository CustomerRepository { get; }
    ITenantRepository TenantRepository { get; }
    IAccountRepository AccountRepository { get; }
    ITransactionRepository TransactionRepository { get; }

    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitAsync(CancellationToken cancellationToken = default);
    Task RollbackAsync(CancellationToken cancellationToken = default);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}