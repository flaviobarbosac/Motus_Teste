namespace Ambev.DeveloperEvaluation.WebApi.Features.Sales.CreateSale;

/// <summary>Uma linha de venda (produto, quantidade, preço unitário).</summary>
public class CreateSaleItemRequest
{
    /// <summary>Produto vendido.</summary>
    public Guid ProductId { get; set; }

    /// <summary>Descrição do produto.</summary>
    public string ProductDescription { get; set; } = string.Empty;

    /// <summary>Quantidade (sujeita a regras de negócio no servidor).</summary>
    public int Quantity { get; set; }

    /// <summary>Preço unitário.</summary>
    public decimal UnitPrice { get; set; }

    /// <summary>Linha anulada.</summary>
    public bool IsCancelled { get; set; }
}
