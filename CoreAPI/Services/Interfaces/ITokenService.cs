using CoreAPI.Models;

namespace CoreAPI.Services.Interfaces
{
    public interface ITokenService
    {
        Task<(string token, DateTime expiresAt)> GenerateToken(User user);
    }
}