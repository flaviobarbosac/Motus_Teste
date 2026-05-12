namespace Ambev.DeveloperEvaluation.WebApi.Common;

/// <summary>
/// Resposta com payload em <see cref="Data"/> além de <see cref="ApiResponse.Success"/> e <see cref="ApiResponse.Message"/>.
/// </summary>
/// <typeparam name="T">Tipo do recurso devolvido.</typeparam>
public class ApiResponseWithData<T> : ApiResponse
{
    /// <summary>Dados principais da operação (ex.: entidade criada ou lida).</summary>
    public T? Data { get; set; }
}
