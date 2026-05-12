namespace Ambev.DeveloperEvaluation.WebApi.Features.Sales.CreateSale;

/// <summary>Resposta após criação de venda (identificador e total calculado).</summary>
public class CreateSaleResponse
{
    /// <summary>Identificador persistido da venda.</summary>
    public Guid Id { get; set; }

    /// <summary>Número da venda.</summary>
    public int SaleNumber { get; set; }

    /// <summary>Total da venda após descontos.</summary>
    public decimal TotalAmount { get; set; }
}
