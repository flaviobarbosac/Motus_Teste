namespace Ambev.DeveloperEvaluation.WebApi.Common;

/// <summary>
/// Lista paginada: <see cref="ApiResponseWithData{T}.Data"/> contém os itens da página atual; metadados de paginação em propriedades dedicadas.
/// </summary>
/// <typeparam name="T">Tipo de cada elemento da coleção.</typeparam>
public class PaginatedResponse<T> : ApiResponseWithData<IEnumerable<T>>
{
    /// <summary>Índice da página atual (1-based).</summary>
    public int CurrentPage { get; set; }
    /// <summary>Número total de páginas disponíveis.</summary>
    public int TotalPages { get; set; }
    /// <summary>Total de registos em todas as páginas.</summary>
    public int TotalCount { get; set; }
}