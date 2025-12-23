using System.Linq.Expressions;
using CoreAPI.DTOs.Customers;
using CoreAPI.Models;

namespace CoreAPI.Repositories.Interfaces;

public interface ICustomerRepository : IRepository<Customer, string>
{
    Task<IEnumerable<Customer>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Customer>> GetAllWithIncludesAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Customer>> GetAllWithFiltering(Expression<Func<Customer, bool>> filtering, CancellationToken cancellationToken = default);
    Task<Customer?> GetByIdWithIncludesAsync(string id, CancellationToken cancellationToken = default);
}