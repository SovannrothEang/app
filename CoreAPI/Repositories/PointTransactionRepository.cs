using CoreAPI.Data;
using CoreAPI.Models;
using CoreAPI.Repositories.Interfaces;

namespace CoreAPI.Repositories;

public class PointTransactionRepository(AppDbContext dbContext) : IPointTransactionRepository
{
    private readonly AppDbContext _dbContext = dbContext;

    public Task<IEnumerable<PointTransaction>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<PointTransaction?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}