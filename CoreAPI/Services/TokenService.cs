using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CoreAPI.Data;
using CoreAPI.Models;
using CoreAPI.Repositories.Interfaces;
using CoreAPI.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace CoreAPI.Services;

public class TokenService(
    IConfiguration config,
    UserManager<User> userManager,
    IUnitOfWork unitOfWork,
    ICurrentUserProvider currentUserProvider) : ITokenService
{
    private readonly IConfiguration _config = config;
    private readonly UserManager<User> _userManager = userManager;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    private readonly ICurrentUserProvider _currentUserProvider = currentUserProvider;
    // private readonly IGenericRepository<TenantUser> _tenantUserRepository = unitOfWork.GetRepository<TenantUser>();

    public async Task<(string, DateTime)> GenerateToken(User user)
    {
        List<Claim> claims = [
            new (JwtRegisteredClaimNames.Sub, user.Id),
            new (JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new (ClaimTypes.NameIdentifier, user.Id),
            new (ClaimTypes.Email, user.Email?? string.Empty),
            new ("tenant_id", user.TenantId)];

        _currentUserProvider.SetTenantId(user.TenantId); // Don't know if it's a violation or not, But need it for Role searching
        var roles = await _userManager.GetRolesAsync(user);
        if (roles.Any())
        {
            claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));
        }
        
        var customer = await _unitOfWork.GetRepository<Customer>().FirstOrDefaultAsync(c => c.UserId == user.Id);
        if (customer != null)
        {
            claims.Add(new Claim("customer_id", customer.Id));
        }

        // Generate keys
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        // Generate Credentials
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        // Generate tokens
        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.Now.AddHours(1),
            signingCredentials: credentials);

        return (new JwtSecurityTokenHandler().WriteToken(token), token.ValidTo);
    }
}
