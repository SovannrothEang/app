using System.Text;
using CoreAPI.Data;
using CoreAPI.Models;
using CoreAPI.Repositories;
using CoreAPI.Repositories.Interfaces;
using CoreAPI.Requirements;
using CoreAPI.Requirements.Handlers;
using CoreAPI.Services;
using CoreAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace CoreAPI;

public static class DependencyInjections
{
    extension(WebApplicationBuilder builder)
    {
        public void AddPersistence()
        {
            builder.Services.AddDbContext<AppDbContext>(
                options => options
                    // .UseSqlServer(builder.Configuration.GetConnectionString("DockerConnection"))
                    .UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
                    // .EnableSensitiveDataLogging() // Development 
            );
        }
        
        public void AddDependencies()
        {
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
            // builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<ITenantRepository, TenantRepository>();
            builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
            builder.Services.AddScoped<ITransactionTypeRepository, TransactionTypeRepository>();

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
            
            builder.Services.AddTransient<IAuthorizationHandler, PlatformRootAccessHandler>();
            builder.Services.AddTransient<IAuthorizationHandler, TenantScopeAccessHandler>();
            builder.Services.AddTransient<IAuthorizationHandler, TenantCustomerAccessHandler>();
            builder.Services.AddTransient<IAuthorizationHandler, CustomerAccessPolicyHandler>();
        }

        public IServiceCollection AddIdentity()
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

        public void AddAuthentication()
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
        
        public void AddAuthorization()
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
}