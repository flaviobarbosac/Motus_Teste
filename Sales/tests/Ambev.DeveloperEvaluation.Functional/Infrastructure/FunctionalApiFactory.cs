using Ambev.DeveloperEvaluation.ORM;
using Ambev.DeveloperEvaluation.WebApi;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Ambev.DeveloperEvaluation.Functional.Infrastructure;

/// <summary>
/// Host da WebApi com SQLite em ficheiro temporário (testes funcionais sem PostgreSQL).
/// </summary>
public sealed class FunctionalApiFactory : WebApplicationFactory<Program>
{
    private readonly string _databasePath = Path.Combine(Path.GetTempPath(), $"sales_func_{Guid.NewGuid():N}.sqlite");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureTestServices(services =>
        {
            RemoveDescriptors(services, typeof(DbContextOptions<DefaultContext>));
            RemoveDescriptors(services, typeof(DefaultContext));

            services.AddDbContext<DefaultContext>(options =>
                options.UseSqlite($"Data Source={_databasePath}"));
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
        foreach (var d in services.Where(d => d.ServiceType == serviceType).ToList())
            services.Remove(d);
    }
}
