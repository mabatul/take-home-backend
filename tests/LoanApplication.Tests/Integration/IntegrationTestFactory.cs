using LoanApplication.Core.Interfaces;
using LoanApplication.Infrastructure.Data;
using LoanApplication.Tests.Fakes;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace LoanApplication.Tests.Integration;

public class IntegrationTestFactory : WebApplicationFactory<Program>
{
    private readonly SqliteConnection _connection;

    public FakeExternalService FakeExternalService { get; } = new();

    public IntegrationTestFactory()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.UseSetting("ApplyMigrationsOnStartup", "false");

        builder.ConfigureServices(services =>
        {
            // Remove the real DbContext registration
            var dbContextDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<LoanDbContext>));
            if (dbContextDescriptor != null)
            {
                services.Remove(dbContextDescriptor);
            }

            var connectionDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(LoanDbContext));
            if (connectionDescriptor != null)
            {
                services.Remove(connectionDescriptor);
            }

            // Register an in-memory SQLite DbContext
            services.AddDbContext<LoanDbContext>(options =>
                options.UseSqlite(_connection));

            // Replace the real external service with a fake
            var externalServiceDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(IExternalService));
            if (externalServiceDescriptor != null)
            {
                services.Remove(externalServiceDescriptor);
            }

            services.AddSingleton<IExternalService>(FakeExternalService);

            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<LoanDbContext>();
            db.Database.EnsureCreated();
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
        {
            _connection.Dispose();
        }
    }
}
