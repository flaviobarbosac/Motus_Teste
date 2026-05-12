namespace Ambev.DeveloperEvaluation.Common.Security;

/// <summary>
/// Extrai o JWT compacto do valor de <c>Authorization</c>, tolerando prefixos <c>Bearer</c> repetidos
/// (ex.: Swagger já envia Bearer e o utilizador cola <c>Bearer eyJ...</c>) e aspas/espacos à volta.
/// </summary>
public static class JwtBearerAuthorizationHeaderNormalizer
{
    private const string BearerPrefix = "Bearer ";

    /// <summary>
    /// Devolve o JWT (três segmentos base64url) ou <c>null</c> se não houver valor utilizável.
    /// </summary>
    public static string? ExtractJwt(string? authorizationHeaderValue)
    {
        if (string.IsNullOrWhiteSpace(authorizationHeaderValue))
            return null;

        var value = authorizationHeaderValue.Trim();

        while (value.StartsWith(BearerPrefix, StringComparison.OrdinalIgnoreCase))
            value = value[BearerPrefix.Length..].Trim();

        value = value.Trim().Trim('"', '\'', '\u201c', '\u201d');

        if (string.IsNullOrWhiteSpace(value))
            return null;

        // JWT compacto: header.payload.signature
        var parts = value.Split('.', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 3)
            return null;

        return value;
    }
}
