using CoreAPI.Models;

namespace CoreAPI.Repositories.Interfaces;

public interface IPointTransactionRepository
{
    Task<IEnumerable<PointTransaction>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<PointTransaction?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
}