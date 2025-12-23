using System.Collections.Concurrent;
using AutoMapper;
using CoreAPI.Data;
using CoreAPI.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore.Storage;

namespace CoreAPI.Repositories;

public class UnitOfWork(AppDbContext context, IMapper mapper) : IUnitOfWork
{
    private readonly AppDbContext _context = context;
    private readonly IMapper _mapper = mapper;
    private readonly ConcurrentDictionary<Type, object> _repositories = [];
    private bool _disposed;
    //private IDbContextTransaction? _objTran;

    //public async Task CommitAsync(CancellationToken ct = default)
    //{
    //    if (_objTran != null)
    //        await _objTran.CommitAsync(ct);
    //}

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        return await _context.SaveChangesAsync(ct);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    //public void Rollback()
    //{
    //    if (_objTran != null)
    //    {
    //        _objTran.Rollback();
    //        _objTran.Dispose();
    //    }
    //}
    public async Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken ct = default)
    {
        return await _context.Database.BeginTransactionAsync(ct);
    }

    // public IGenericRepository<T> GetRepository<T>() where T : class
    // {
    //     var type = typeof(T);
    //     return (IGenericRepository<T>)_repositories.GetOrAdd(type, _ =>
    //         new GenericRepository<T>(_context, _mapper));
    // }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            //if (_objTran != null)
            //{
            //    _objTran.Dispose();
            //    _objTran = null;
            //}
            _context.Dispose();
            _disposed = true;
        }
    }
}
