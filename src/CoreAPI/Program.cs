using Application.Mappings;
using Application.Middlewares;
using CoreAPI;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.OpenApi;
using Scalar.AspNetCore;
using Microsoft.AspNetCore.HttpsPolicy;
using Serilog;
using Serilog.Events;
using Domain.Exceptions;
using Infrastructure;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, service, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .Enrich.FromLogContext()                       // Vital for correlation IDs
        .Filter.ByExcluding(e =>
            e.Properties.ContainsKey("Sql")
            || (e.Properties.ContainsKey("SourceContext")
                && e.Properties["SourceContext"].ToString().Contains("Microsoft.EntityFrameworkCore.Database.Command")))
        .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", LogEventLevel.Warning)
        .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3} {RequestId}] {Message:lj}{NewLine}{Exception}") // Write to Console (Docker/Dev)
        .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)); // Write to File
    
    builder.Services
        .AddControllers()
        .AddJsonOptions(opt =>
        {
            // Keep original property names during serialization/deserialization.
            opt.JsonSerializerOptions.PropertyNamingPolicy = null;
        });
    builder.Services.AddAutoMapper(cfg =>
    {
        cfg.AddProfile<AccountProfile>();
        cfg.AddProfile<CustomerProfile>();
        cfg.AddProfile<IdentityProfile>();
        cfg.AddProfile<TenantProfile>();
        cfg.AddProfile<TransactionProfile>();
        cfg.AddProfile<TransactionTypeProfile>();

        cfg.AllowNullCollections = true;
    }, typeof(Program).Assembly);

    builder.AddPersistence();
    builder.AddDependencies();
    builder.AddIdentity();

    // builder.Services.AddOpenApi();
    builder.Services.AddSwaggerGen(options =>
    {
        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT"
        });
    });

    builder.Services.AddValidatorsFromAssemblyContaining<Program>();

    builder.Services.AddHttpContextAccessor();
    builder.Services.AddProblemDetails();
    builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
    builder.Services.AddEndpointsApiExplorer();

    builder.AddAuthentication();
    builder.AddAuthorization();
    builder.Services.Configure<HstsOptions>(options =>
    {
        options.MaxAge = TimeSpan.FromDays(30);
        options.IncludeSubDomains = true;
    });

    var app = builder.Build();

    using (var scope = app.Services.CreateScope())
    {
        await IdentitySeeder.SeedAsync(scope.ServiceProvider);
    }

    if (app.Environment.IsDevelopment())
    {
        // app.MapOpenApi();
        app.MapSwagger("/openapi/{documentName}.json");
        app.MapScalarApiReference(options =>
        {
            options.Servers = [];
            options.Authentication = new ScalarAuthenticationOptions
                { PreferredSecuritySchemes = [IdentityConstants.BearerScheme] };
        });
        app.Map("/", () => Results.Redirect("/scalar"));
    }

    // app.UseMiddleware<ErrorHandlingMiddleware>();
    app.UseMiddleware<TaskCanceledMiddleware>();
    app.UseHsts();
    app.UseExceptionHandler();
    app.UseSerilogRequestLogging();

    app.UseHttpsRedirection();
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();

    app.Run();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}