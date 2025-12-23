using System.Collections.Concurrent;
using AutoMapper;
using CoreAPI.Data;
using CoreAPI.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore.Storage;

namespace CoreAPI.Repositories;

public sealed class UnitOfWork(AppDbContext context, IMapper mapper) : IUnitOfWork
{
    private readonly AppDbContext _context = context;
    private readonly IMapper _mapper = mapper;
    // private readonly ConcurrentDictionary<Type, object> _repositories = [];
    private bool _disposed;
    private IDbContextTransaction? _transaction = null;

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        _transaction ??= await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            try
            {
                await _context.SaveChangesAsync(cancellationToken);
                await _transaction.CommitAsync(cancellationToken);
            }
            catch
            {
                await RollbackAsync(cancellationToken);
                throw;
            }
            finally
            {
                _transaction.Dispose();
            }
        }
    }

    public async Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync(cancellationToken);
            await _transaction.DisposeAsync();
        }
    }

    public IUserRepository UserRepository { get; private set; } = new UserRepository(context);
    public ICustomerRepository CustomerRepository { get; private set; } = new CustomerRepository(context);
    public ITenantRepository TenantRepository { get; private set; } = new TenantRepository(context);
    public ILoyaltyAccountRepository LoyaltyAccountRepository { get; private set; } = new LoyaltyAccountRepository(context);
    public IPointTransactionRepository PointTransactionRepository { get; private set; } = new PointTransactionRepository(context);

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        return await _context.SaveChangesAsync(ct);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    // public IGenericRepository<T> GetRepository<T>() where T : class
    // {
    //     var type = typeof(T);
    //     return (IGenericRepository<T>)_repositories.GetOrAdd(type, _ =>
    //         new GenericRepository<T>(_context, _mapper));
    // }

    private void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            if (_transaction != null)
            {
                _transaction.Dispose();
                _transaction = null;
            }
            _context.Dispose();
            _disposed = true;
        }
    }
}
