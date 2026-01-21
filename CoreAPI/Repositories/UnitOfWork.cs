using System.Collections.Concurrent;
using AutoMapper;
using CoreAPI.Data;
using CoreAPI.Models.Shared;
using CoreAPI.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore.Storage;

namespace CoreAPI.Repositories;

public sealed class UnitOfWork(
    AppDbContext context,
    IServiceProvider serviceProvider) : IUnitOfWork
{
    private readonly AppDbContext _context = context;

    private readonly ConcurrentDictionary<Type, object> _repositories = [];
    private bool _disposed;
    
    public IUserRepository UserRepository => serviceProvider.GetRequiredService<IUserRepository>();
    public ITenantRepository TenantRepository => serviceProvider.GetRequiredService<ITenantRepository>();
    public ITransactionRepository TransactionRepository => serviceProvider.GetRequiredService<ITransactionRepository>();
    public ITransactionTypeRepository TransactionTypeRepository => serviceProvider.GetRequiredService<ITransactionTypeRepository>();
    public IAccountTypeRepository AccountTypeRepository => serviceProvider.GetRequiredService<IAccountTypeRepository>();

    public async Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        _context.ChangeTracker.Clear();
        return await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task<int> CompleteAsync(CancellationToken ct = default)
    {
        return await _context.SaveChangesAsync(ct);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    public IRepository<T> GetRepository<T>() where T : class, IAuditEntity
    {
        var type = typeof(T);
        return (IRepository<T>)_repositories.GetOrAdd(type, _ => new Repository<T>(_context, serviceProvider.GetRequiredService<IMapper>()));
    }

    private void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            _context.Dispose();
            _disposed = true;
        }
    }
}
