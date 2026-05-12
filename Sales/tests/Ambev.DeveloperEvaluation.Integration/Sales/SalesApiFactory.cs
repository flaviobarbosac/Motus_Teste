using Ambev.DeveloperEvaluation.ORM;
using Ambev.DeveloperEvaluation.WebApi;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Ambev.DeveloperEvaluation.Integration.Sales;

/// <summary>
/// Host real da WebApi com SQLite (ficheiro temporário) no lugar de PostgreSQL para testes HTTP com transações relacionais.
/// </summary>
public class SalesApiFactory : WebApplicationFactory<Program>
{
    private readonly string _databasePath = Path.Combine(Path.GetTempPath(), $"sales_e2e_{Guid.NewGuid():N}.sqlite");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureTestServices(services =>
        {
            RemoveDescriptors(services, typeof(DbContextOptions<DefaultContext>));
            RemoveDescriptors(services, typeof(DefaultContext));

            var connectionString = $"Data Source={_databasePath}";
            services.AddDbContext<DefaultContext>(options =>
                options.UseSqlite(connectionString));
        });
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            try
            {
                if (File.Exists(_databasePath))
                    File.Delete(_databasePath);
            }
            catch
            {
                // ignorar bloqueio temporário em Windows
            }
        }

        base.Dispose(disposing);
    }

    private static void RemoveDescriptors(IServiceCollection services, Type serviceType)
    {
        var list = services.Where(d => d.ServiceType == serviceType).ToList();
        foreach (var d in list)
            services.Remove(d);
    }
}
