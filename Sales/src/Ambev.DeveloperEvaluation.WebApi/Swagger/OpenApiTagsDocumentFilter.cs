using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Linq;

namespace Ambev.DeveloperEvaluation.WebApi.Swagger;

/// <summary>
/// Associa descrições às tags do OpenAPI (agrupamento por controlador no Swagger UI).
/// </summary>
public sealed class OpenApiTagsDocumentFilter : IDocumentFilter
{
    private static readonly Dictionary<string, string> TagDescriptions = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Auth"] =
            "Autenticação: obtenha o JWT com email e palavra-passe. Não requer cabeçalho Authorization. " +
            "Use o token devolvido em `data.token` no botão **Authorize** (esquema Bearer) para aceder a Sales e Users.",
        ["Users"] =
            "Gestão de utilizadores: registo público (POST), listagem, consulta e eliminação. " +
            "Exceto criação de utilizador, todos os métodos exigem JWT válido.",
        ["Sales"] =
            "Gestão de vendas (CRUD): criação com linhas, totais e descontos calculados no servidor; listagem paginada; consulta, atualização e eliminação. " +
            "Todos os endpoints exigem JWT válido."
    };

    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        swaggerDoc.Tags ??= new List<OpenApiTag>();

        foreach (var (name, description) in TagDescriptions)
        {
            var tag = swaggerDoc.Tags.FirstOrDefault(t => string.Equals(t.Name, name, StringComparison.Ordinal));
            if (tag is not null)
                tag.Description = description;
            else
                swaggerDoc.Tags.Add(new OpenApiTag { Name = name, Description = description });
        }
    }
}
