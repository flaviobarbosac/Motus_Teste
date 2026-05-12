namespace Ambev.DeveloperEvaluation.WebApi.Features.Sales.CreateSale;

/// <summary>
/// Corpo para criar ou atualizar uma venda (cabeçalho + linhas). Totais e descontos são calculados no servidor.
/// </summary>
public class CreateSaleRequest
{
    /// <summary>Número comercial único da venda.</summary>
    public int SaleNumber { get; set; }

    /// <summary>Data/hora da venda (UTC recomendado).</summary>
    public DateTime SaleDate { get; set; }

    /// <summary>Identificador do cliente.</summary>
    public Guid CustomerId { get; set; }

    /// <summary>Nome do cliente (exibição).</summary>
    public string CustomerName { get; set; } = string.Empty;

    /// <summary>Identificador da filial.</summary>
    public Guid BranchId { get; set; }

    /// <summary>Nome da filial.</summary>
    public string BranchName { get; set; } = string.Empty;

    /// <summary>Indica se a venda está anulada.</summary>
    public bool IsCancelled { get; set; }

    /// <summary>Linhas da venda (produto, quantidades e preços).</summary>
    public IList<CreateSaleItemRequest> Items { get; set; } = new List<CreateSaleItemRequest>();
}
