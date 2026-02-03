using Domain.Entities;

namespace Application.Services
{
    public interface ITokenService
    {
        Task<(string token, DateTime expiresAt)> GenerateToken(User user);
    }
}