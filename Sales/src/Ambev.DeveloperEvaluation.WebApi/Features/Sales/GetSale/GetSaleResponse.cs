namespace Ambev.DeveloperEvaluation.WebApi.Features.Sales.GetSale;

/// <summary>Linha de venda devolvida pela API (inclui desconto e total da linha).</summary>
public class SaleItemResponse
{
    /// <summary>Identificador da linha.</summary>
    public Guid Id { get; set; }

    /// <summary>Produto.</summary>
    public Guid ProductId { get; set; }

    /// <summary>Descrição.</summary>
    public string ProductDescription { get; set; } = string.Empty;

    /// <summary>Quantidade.</summary>
    public int Quantity { get; set; }

    /// <summary>Preço unitário.</summary>
    public decimal UnitPrice { get; set; }

    /// <summary>Valor de desconto aplicado na linha.</summary>
    public decimal DiscountAmount { get; set; }

    /// <summary>Total da linha após desconto.</summary>
    public decimal LineTotal { get; set; }

    /// <summary>Linha anulada.</summary>
    public bool IsCancelled { get; set; }
}

/// <summary>Venda completa para leitura (cabeçalho, total e linhas).</summary>
public class GetSaleResponse
{
    /// <summary>Identificador da venda.</summary>
    public Guid Id { get; set; }

    /// <summary>Número da venda.</summary>
    public int SaleNumber { get; set; }

    /// <summary>Data da venda.</summary>
    public DateTime SaleDate { get; set; }

    /// <summary>Cliente.</summary>
    public Guid CustomerId { get; set; }

    /// <summary>Nome do cliente.</summary>
    public string CustomerName { get; set; } = string.Empty;

    /// <summary>Filial.</summary>
    public Guid BranchId { get; set; }

    /// <summary>Nome da filial.</summary>
    public string BranchName { get; set; } = string.Empty;

    /// <summary>Total da venda.</summary>
    public decimal TotalAmount { get; set; }

    /// <summary>Venda anulada.</summary>
    public bool IsCancelled { get; set; }

    /// <summary>Linhas da venda.</summary>
    public IList<SaleItemResponse> Items { get; set; } = new List<SaleItemResponse>();
}
