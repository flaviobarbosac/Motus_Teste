using Ambev.DeveloperEvaluation.Application;
using Ambev.DeveloperEvaluation.Common.HealthChecks;
using Ambev.DeveloperEvaluation.Common.Logging;
using Ambev.DeveloperEvaluation.Common.Security;
using Ambev.DeveloperEvaluation.Common.Validation;
using Ambev.DeveloperEvaluation.IoC;
using Ambev.DeveloperEvaluation.ORM;
using Ambev.DeveloperEvaluation.WebApi.Middleware;
using Ambev.DeveloperEvaluation.WebApi.Swagger;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Reflection;
using System.Text.Json;

namespace Ambev.DeveloperEvaluation.WebApi;

public class Program
{
    public static void Main(string[] args)
    {
        try
        {
            Log.Information("Starting web application");

            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
            builder.AddDefaultLogging();

            builder.Services.AddControllers()
                .AddJsonOptions(o =>
                {
                    o.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                    o.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
                });
            builder.Services.AddEndpointsApiExplorer();

            builder.AddBasicHealthChecks();
            builder.Services.AddSwaggerGen(options =>
            {
                var assemblyName = Assembly.GetExecutingAssembly().GetName().Name;
                var xmlFile = $"{assemblyName}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                if (File.Exists(xmlPath))
                    options.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);

                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "API de avaliação — Vendas e utilizadores",
                    Version = "v1",
                    Description = """
                        **Fluxo típico**
                        1. `POST /api/users` — regista um utilizador (público).
                        2. `POST /api/auth` — obtém JWT (`data.token`).
                        3. **Authorize** no Swagger — cole o token (ou `Bearer` + token); em clientes HTTP use `Authorization: Bearer <token>`.
                        4. `GET/POST/PUT/DELETE /api/sales` e restantes `GET/DELETE /api/users` — requerem JWT.

                        **Respostas**
                        - Sucesso com dados: `success`, `message`, `data` (ver cada operação).
                        - Listagens paginadas: `data` (array), `currentPage`, `totalPages`, `totalCount`.
                        - Erros de validação: HTTP 400 com detalhes em `errors`.

                        **Saúde (fora do Swagger de controladores)**  
                        `GET /health`, `/health/live`, `/health/ready` — estado da aplicação.

                        **Segurança**  
                        Em produção configure `Jwt:SecretKey`, `Jwt:Issuer` e `Jwt:Audience` de forma consistente.
                        """
                });

                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description =
                        "JWT obtido em `POST /api/auth` (campo `data.token`). " +
                        "Cole só o token `eyJ...` ou `Bearer eyJ...` — prefixos `Bearer` repetidos são normalizados. " +
                        "`Jwt:SecretKey`, `Issuer` e `Audience` devem coincidir entre emissão e validação.",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT"
                });
                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });

                options.DocumentFilter<OpenApiTagsDocumentFilter>();
            });

            builder.Services.AddDbContext<DefaultContext>(options =>
                options.UseNpgsql(
                    builder.Configuration.GetConnectionString("DefaultConnection"),
                    b => b.MigrationsAssembly("Ambev.DeveloperEvaluation.ORM")
                )
            );

            builder.Services.AddJwtAuthentication(builder.Configuration);

            builder.RegisterDependencies();

            builder.Services.AddAutoMapper(typeof(Program).Assembly, typeof(ApplicationLayer).Assembly);

            builder.Services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssemblies(
                    typeof(ApplicationLayer).Assembly,
                    typeof(Program).Assembly
                );
            });

            builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

            var app = builder.Build();
            app.UseMiddleware<ValidationExceptionMiddleware>();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.EnablePersistAuthorization();
                    c.DocumentTitle = "API Vendas — documentação";
                });
            }

            if (!app.Environment.IsEnvironment("Testing"))
                app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseBasicHealthChecks();

            app.MapControllers();

            app.Run();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Application terminated unexpectedly");
            throw;
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
}
