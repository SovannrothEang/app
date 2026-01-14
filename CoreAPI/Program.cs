using CoreAPI;
using CoreAPI.Data;
using CoreAPI.Exceptions;
using CoreAPI.Middlewares;
using CoreAPI.Profiles;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.OpenApi;
using Scalar.AspNetCore;
using Microsoft.AspNetCore.HttpsPolicy;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);
    
    builder.Host.UseSerilog((context, service, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .Enrich.FromLogContext()                       // Vital for correlation IDs
        .WriteTo.Console()                             // Write to Console (Docker/Dev)
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
        cfg.AddProfile<IdentityProfile>();
        cfg.AddProfile<TenantProfile>();
        cfg.AddProfile<CustomerProfile>();
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