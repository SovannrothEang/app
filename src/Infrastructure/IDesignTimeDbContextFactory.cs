using Application.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

public sealed class DesignTimeAppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var basePath = Directory.GetCurrentDirectory();

        var coreApiPath = Path.Combine(basePath, "..", "CoreAPI");
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.Exists(coreApiPath) ? coreApiPath : basePath)
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(configuration.GetConnectionString("DefaultConnection"))
            .EnableSensitiveDataLogging(false)
            .Options;

        var services = new ServiceCollection();
        services.AddSingleton<ICurrentUserProvider, DesignTimeCurrentUserProvider>();

        var serviceProvider = services.BuildServiceProvider();

        return new AppDbContext(options, serviceProvider, configuration);
    }

    private sealed class DesignTimeCurrentUserProvider : ICurrentUserProvider
    {
        public string? UserId => null;
        public string? Email => null;
        public string? TenantId => null;
        public string? CustomerId => null;
        public void SetTenantId(string tenantId) { }

        public bool IsAuthenticated => false;
        public bool IsInRole(string role) { return true; }

        public IReadOnlyList<string> Roles => [];
    }
}