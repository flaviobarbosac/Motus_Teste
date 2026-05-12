using System.Net;
using Ambev.DeveloperEvaluation.Functional.Infrastructure;
using Ambev.DeveloperEvaluation.ORM;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Ambev.DeveloperEvaluation.Functional.Health;

/// <summary>
/// Testes funcionais mínimos (pipeline HTTP + host) sem exercer regras de negócio de vendas.
/// </summary>
public class HealthEndpointsFunctionalTests : IClassFixture<FunctionalApiFactory>
{
    private readonly FunctionalApiFactory _factory;

    public HealthEndpointsFunctionalTests(FunctionalApiFactory factory) => _factory = factory;

    private static async Task EnsureSchemaAsync(FunctionalApiFactory factory)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<DefaultContext>();
        await db.Database.EnsureCreatedAsync();
    }

    [Theory]
    [InlineData("/health/live")]
    [InlineData("/health/ready")]
    [InlineData("/health")]
    public async Task Health_endpoints_return_ok(string path)
    {
        await EnsureSchemaAsync(_factory);
        var client = _factory.CreateClient();
        var response = await client.GetAsync(path);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
