using System.Linq.Expressions;
using CoreAPI.Models;

namespace CoreAPI.Repositories.Interfaces;

public interface IUserRepository : IRepository<User>
{
    Task<IEnumerable<User>> GetAllAsync(Expression<Func<User, bool>>? filtering = null,
        CancellationToken cancellationToken = default);
    Task<User?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    Task AddToRoleAsync(string userId, string roleId, string tenantId);

    Task<IEnumerable<string>> GetAllRolesAsync(
        string userId,
        CancellationToken cancellationToken = default);
}