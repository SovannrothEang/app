using CoreAPI.DTOs.Transactions;
using CoreAPI.Models;

namespace CoreAPI.Repositories.Interfaces;

public interface ITransactionTypeRepository
{
    Task<IEnumerable<TransactionType>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<OperationDto>> GetAllOperationsAsync(CancellationToken cancellationToken = default);
    Task<TransactionType?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<TransactionType?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<TransactionType?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<bool> IsExistAsync(string id, CancellationToken cancellationToken = default);
    Task<bool> IsTypeExistAsync(string name, CancellationToken cancellationToken = default);
    Task CreateAsync(TransactionType type, CancellationToken cancellationToken = default);
    Task<int> CreateBatchAsync(
        IEnumerable<TransactionType> types,
        CancellationToken cancellationToken = default);
}