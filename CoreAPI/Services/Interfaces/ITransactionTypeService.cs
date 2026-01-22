using CoreAPI.DTOs.Transactions;

namespace CoreAPI.Services.Interfaces;

public interface ITransactionTypeService
{
    Task<IEnumerable<TransactionTypeDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<TransactionTypeDto?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<IEnumerable<OperationDto>> GetAllOperationsAsync(CancellationToken cancellationToken = default);
    Task<TransactionTypeDto?> GetByNameAsync(string name,  CancellationToken cancellationToken = default);
    Task<TransactionTypeDto?> GetBySlugAsync(string slug,  CancellationToken cancellationToken = default);
    Task<bool> IsExistAsync(string id, CancellationToken cancellationToken = default);
    Task<bool> IsTypeExistAsync(string name, CancellationToken cancellationToken = default);
    Task<TransactionTypeDto> CreateAsync(TransactionTypeCreateDto dto, CancellationToken cancellationToken = default);
    Task<int> CreateBatchAsync(IEnumerable<TransactionTypeCreateDto> dtos, CancellationToken cancellationToken = default);
}
