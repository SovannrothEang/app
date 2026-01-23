using Microsoft.EntityFrameworkCore.Storage;

namespace CoreAPI.Repositories.Interfaces;

public interface IUnitOfWork : IDisposable
{
    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
    IRepository<T> GetRepository<T>() where T : class;
    Task<int> CompleteAsync(CancellationToken cancellationToken = default);
}