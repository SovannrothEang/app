using CoreAPI.Models;

namespace CoreAPI.Services;

public interface IPointTransactionService
{

    Task<IEnumerable<PointTransaction>> GetAllTransactionsAsync(CancellationToken cancellationToken = default);
}

public class PointTransactionService() : IPointTransactionService
{
    public Task<IEnumerable<PointTransaction>> GetAllTransactionsAsync(CancellationToken cancellationToken = default)
    {
        // var transactions = await _pointTransactionRepository.GetAllAsync(
        //     orderBy: e => e.OrderBy(p => p.OccurredOn),
        //     cancellationToken: cancellationToken);
        // return transactions;
        throw new NotImplementedException();
    }
}