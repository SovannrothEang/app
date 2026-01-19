using System.Linq.Expressions;
using CoreAPI.Models;

namespace CoreAPI.Repositories.Interfaces;

public interface ICustomerRepository 
{
    Task<IEnumerable<Customer>> GetAllAsync(
        bool childIncluded = false,
        Expression<Func<Customer, bool>>? filtering = null,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<Customer>> GetAllForTenantAsync(
        bool childIncluded = false,
        Expression<Func<Customer, bool>>? filtering = null,
        CancellationToken cancellationToken = default);
    Task<Customer?> GetByIdForCustomerAsync(
        string id,
        bool childIncluded = false,
        bool trackChanges = false,
        CancellationToken cancellationToken = default);

    Task<Customer?> GetByIdForTenantAsync(
        string id,
        bool childIncluded = false,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(string id, CancellationToken cancellationToken = default);
    Task<bool> ExistsInTenantAsync(string id, CancellationToken cancellationToken = default);
}