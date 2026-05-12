using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Ambev.DeveloperEvaluation.Domain;
using Ambev.DeveloperEvaluation.ORM;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Ambev.DeveloperEvaluation.Integration.Sales;

internal static class JsonElementExtensions
{
    internal static JsonElement Prop(this JsonElement el, string name)
    {
        foreach (var p in el.EnumerateObject())
        {
            if (string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase))
                return p.Value;
        }

        throw new KeyNotFoundException(
            $"JSON property '{name}' not found. Available: {string.Join(", ", el.EnumerateObject().Select(p => p.Name))}.");
    }
}

/// <summary>
/// Validação física dos endpoints /api/sales (HTTP real, host de teste, EF InMemory).
/// </summary>
public class SalesEndpointsIntegrationTests : IClassFixture<SalesApiFactory>
{
    private readonly SalesApiFactory _factory;
    private readonly JsonSerializerOptions _json = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public SalesEndpointsIntegrationTests(SalesApiFactory factory) => _factory = factory;

    private static async Task<HttpClient> CreateApiClientAsync(SalesApiFactory factory)
    {
        await EnsureSchemaAsync(factory);
        return factory.CreateClient();
    }

    private static async Task EnsureSchemaAsync(SalesApiFactory factory)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<DefaultContext>();
        await db.Database.EnsureCreatedAsync();
    }

    [Fact]
    public async Task Post_Get_List_Put_Delete_sales_endpoints_return_expected_statuses()
    {
        var client = await CreateApiClientAsync(_factory);
        var saleNumber = Random.Shared.Next(900_000, 999_999);
        var customerId = Guid.NewGuid();
        var branchId = Guid.NewGuid();
        var productId = Guid.NewGuid();

        var createBody = new
        {
            saleNumber,
            saleDate = DateTime.UtcNow,
            customerId,
            customerName = "Cliente Teste",
            branchId,
            branchName = "Filial Teste",
            isCancelled = false,
            items = new[]
            {
                new { productId, productDescription = "Produto A", quantity = 4, unitPrice = 100m, isCancelled = false }
            }
        };

        var createResponse = await client.PostAsJsonAsync("/api/sales", createBody, _json);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var createJson = await createResponse.Content.ReadAsStringAsync();
        using var createDoc = JsonDocument.Parse(createJson);
        var saleId = createDoc.RootElement.Prop("data").Prop("id").GetGuid();
        var totalFromCreate = createDoc.RootElement.Prop("data").Prop("totalAmount").GetDecimal();
        Assert.True(totalFromCreate > 0);

        var getResponse = await client.GetAsync($"/api/sales/{saleId}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        using var getDoc = JsonDocument.Parse(await getResponse.Content.ReadAsStringAsync());
        var getRoot = getDoc.RootElement;
        Assert.Equal(saleNumber, getRoot.Prop("data").Prop("saleNumber").GetInt32());
        Assert.Equal(customerId, getRoot.Prop("data").Prop("customerId").GetGuid());
        Assert.Single(getRoot.Prop("data").Prop("items").EnumerateArray());

        var listResponse = await client.GetAsync("/api/sales?page=1&pageSize=10");
        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);
        using var listDoc = JsonDocument.Parse(await listResponse.Content.ReadAsStringAsync());
        var listRoot = listDoc.RootElement;
        Assert.True(listRoot.Prop("success").GetBoolean());
        Assert.True(listRoot.Prop("totalCount").GetInt32() >= 1);

        var updateBody = new
        {
            saleNumber,
            saleDate = DateTime.UtcNow,
            customerId,
            customerName = "Cliente Atualizado",
            branchId,
            branchName = "Filial Teste",
            isCancelled = false,
            items = new[]
            {
                new { productId, productDescription = "Produto A", quantity = 2, unitPrice = 50m, isCancelled = false }
            }
        };

        var putResponse = await client.PutAsJsonAsync($"/api/sales/{saleId}", updateBody, _json);
        Assert.Equal(HttpStatusCode.OK, putResponse.StatusCode);

        var deleteResponse = await client.DeleteAsync($"/api/sales/{saleId}");
        Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);

        var getAfterDelete = await client.GetAsync($"/api/sales/{saleId}");
        Assert.Equal(HttpStatusCode.NotFound, getAfterDelete.StatusCode);
    }

    [Fact]
    public async Task Post_duplicate_sale_number_returns_409()
    {
        var client = await CreateApiClientAsync(_factory);
        var saleNumber = Random.Shared.Next(800_000, 899_999);
        var body = new
        {
            saleNumber,
            saleDate = DateTime.UtcNow,
            customerId = Guid.NewGuid(),
            customerName = "C",
            branchId = Guid.NewGuid(),
            branchName = "B",
            isCancelled = false,
            items = new[] { new { productId = Guid.NewGuid(), productDescription = "P", quantity = 1, unitPrice = 1m, isCancelled = false } }
        };

        var first = await client.PostAsJsonAsync("/api/sales", body, _json);
        Assert.Equal(HttpStatusCode.Created, first.StatusCode);

        var second = await client.PostAsJsonAsync("/api/sales", body, _json);
        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
    }

    [Fact]
    public async Task Get_sales_list_without_token_returns_ok()
    {
        var client = await CreateApiClientAsync(_factory);
        var response = await client.GetAsync("/api/sales?page=1&pageSize=10");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Get_users_route_casing_returns_200()
    {
        var client = await CreateApiClientAsync(_factory);
        var lower = await client.GetAsync("/api/users?page=1&pageSize=10");
        var mixed = await client.GetAsync("/api/Users?page=1&pageSize=10");
        Assert.Equal(HttpStatusCode.OK, lower.StatusCode);
        Assert.Equal(HttpStatusCode.OK, mixed.StatusCode);
    }

    [Fact]
    public async Task Get_sales_without_query_returns_first_page_with_default_page_size_and_correct_total_count()
    {
        var client = await CreateApiClientAsync(_factory);
        var beforeJson = await (await client.GetAsync("/api/sales?pageSize=10000&page=1")).Content.ReadAsStringAsync();
        using var beforeDoc = JsonDocument.Parse(beforeJson);
        var beforeTotal = beforeDoc.RootElement.Prop("totalCount").GetInt32();

        const int n = 12;
        var baseNumber = Random.Shared.Next(600_000, 650_000);
        for (var i = 0; i < n; i++)
        {
            var body = new
            {
                saleNumber = baseNumber + i,
                saleDate = DateTime.UtcNow,
                customerId = Guid.NewGuid(),
                customerName = "C",
                branchId = Guid.NewGuid(),
                branchName = "B",
                isCancelled = false,
                items = new[] { new { productId = Guid.NewGuid(), productDescription = "P", quantity = 1, unitPrice = 1m, isCancelled = false } }
            };
            var res = await client.PostAsJsonAsync("/api/sales", body, _json);
            Assert.True(res.IsSuccessStatusCode, await res.Content.ReadAsStringAsync());
        }

        var listRes = await client.GetAsync("/api/sales");
        Assert.Equal(HttpStatusCode.OK, listRes.StatusCode);
        using var listDoc = JsonDocument.Parse(await listRes.Content.ReadAsStringAsync());
        var root = listDoc.RootElement;
        var total = root.Prop("totalCount").GetInt32();
        Assert.Equal(beforeTotal + n, total);

        var data = root.Prop("data");
        var expectedOnFirstPage = Math.Min(total, SalesListPagination.DefaultPageSize);
        Assert.Equal(expectedOnFirstPage, data.GetArrayLength());
    }
}
