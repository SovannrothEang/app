using Application;
using Application.Security.Handlers;
using Application.Security.Requirements;
using Application.Services;
using System.Text;
using Domain.Entities;
using Domain.Repositories;
using Infrastructure;
using Infrastructure.Repositories;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace CoreAPI;

public static class DependencyInjections
{
    public static void AddPersistence(this WebApplicationBuilder builder)
    {
        builder.Services.AddDbContext<AppDbContext>(
            options => options
                // .UseSqlServer(builder.Configuration.GetConnectionString("DockerConnection"))
                .UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
                .EnableSensitiveDataLogging(false) // Development 
        );
    }
    
    public static void AddDependencies(this WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
        builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

        // Application Services
        builder.Services.AddScoped<ICustomerService, CustomerService>();
        builder.Services.AddScoped<ITenantService, TenantService>();
        builder.Services.AddScoped<IAccountService, AccountService>();
        builder.Services.AddScoped<IAccountTypeService, AccountTypeService>();
        builder.Services.AddScoped<ITransactionService, TransactionService>();
        builder.Services.AddScoped<ITransactionTypeService, TransactionTypeService>();
    
        builder.Services.AddScoped<ITokenService, TokenService>();
        builder.Services.AddScoped<IRoleService, RoleService>();
        builder.Services.AddScoped<IUserService, AuthService>();
        builder.Services.AddScoped<ICurrentUserProvider, CurrentUserProvider>();
        
        // Authorization Handlers
        builder.Services.AddTransient<IAuthorizationHandler, PlatformRootAccessHandler>();
        builder.Services.AddTransient<IAuthorizationHandler, TenantScopeAccessHandler>();
        builder.Services.AddTransient<IAuthorizationHandler, TenantCustomerAccessHandler>();
        builder.Services.AddTransient<IAuthorizationHandler, CustomerAccessPolicyHandler>();
    }

    public static IServiceCollection AddIdentity(this WebApplicationBuilder builder)
    {
        builder.Services.AddIdentity<User, Role>(options =>
            {
                options.Password.RequiredLength = 8;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireDigit = true;
            })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();
        
        return builder.Services;
    }

    public static void AddAuthentication(this WebApplicationBuilder builder)
    {
        var jwtIssuer = builder.Configuration["Jwt:Issuer"];
        var jwtAudience = builder.Configuration["Jwt:Audience"];
        var jwtKey = builder.Configuration["Jwt:Key"]
                     ?? throw new InvalidOperationException("Jwt key hasn't been configured yet!");
        
        builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme= JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme= JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = !string.IsNullOrWhiteSpace(jwtIssuer),
                    ValidateAudience = !string.IsNullOrWhiteSpace(jwtAudience),
                    ValidateIssuerSigningKey = true,
                    RequireExpirationTime = true,
                    ValidIssuer = jwtIssuer,
                    ValidAudience = jwtAudience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                };
            });
    }
    
    public static void AddAuthorization(this WebApplicationBuilder builder)
    {
        builder.Services.AddAuthorizationBuilder()
            .AddPolicy(Constants.RequireAuthenticatedUser, p =>
                p.RequireAuthenticatedUser())
            .AddPolicy(Constants.PlatformRootAccessPolicy, p =>
                p.Requirements.Add(new PlatformRootAccessRequirement()))
            .AddPolicy(Constants.TenantCustomerAccessPolicy, p =>
                p.Requirements.Add(new TenantCustomerAccessRequirement()))
            .AddPolicy(Constants.TenantScopeAccessPolicy, p =>
                p.Requirements.Add(new TenantScopeAccessRequirement()))
            .AddPolicy(Constants.CustomerAccessPolicy, p =>
                p.Requirements.Add(new CustomerAccessPolicyRequirement()));
    }
}