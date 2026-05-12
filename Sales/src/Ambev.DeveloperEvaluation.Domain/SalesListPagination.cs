namespace Ambev.DeveloperEvaluation.Domain;

/// <summary>
/// Limites da listagem de vendas (GET /api/sales) — um pedido pode trazer até <see cref="MaxPageSize"/> registos.
/// </summary>
public static class SalesListPagination
{
    /// <summary>Quando o cliente não envia pageSize.</summary>
    public const int DefaultPageSize = 1000;

    /// <summary>Máximo permitido por pedido (proteção de memória).</summary>
    public const int MaxPageSize = 10000;
}
