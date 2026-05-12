using Ambev.DeveloperEvaluation.Common.Validation;

namespace Ambev.DeveloperEvaluation.WebApi.Common;

/// <summary>
/// Envelope genérico de resposta (sucesso ou erro sem payload principal).
/// </summary>
public class ApiResponse
{
    /// <summary>Indica se a operação foi bem-sucedida.</summary>
    public bool Success { get; set; }
    /// <summary>Mensagem legível (erro ou confirmação).</summary>
    public string Message { get; set; } = string.Empty;
    /// <summary>Detalhes de validação quando <see cref="Success"/> é falso e existem erros de campo.</summary>
    public IEnumerable<ValidationErrorDetail> Errors { get; set; } = [];
}
