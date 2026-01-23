using CoreAPI.Models.Shared;
using Microsoft.EntityFrameworkCore.Storage;

namespace CoreAPI.Repositories.Interfaces;

public interface IUnitOfWork : IDisposable
{
    ITransactionRepository TransactionRepository { get; }

    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
    IRepository<T> GetRepository<T>() where T : class;
    Task<int> CompleteAsync(CancellationToken cancellationToken = default);
}