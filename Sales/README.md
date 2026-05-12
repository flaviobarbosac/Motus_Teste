# Developer Evaluation Project

`READ CAREFULLY`

## Use Case
**You are a developer on the DeveloperStore team. Now we need to implement the API prototypes.**

As we work with `DDD`, to reference entities from other domains, we use the `External Identities` pattern with denormalization of entity descriptions.

Therefore, you will write an API (complete CRUD) that handles sales records. The API needs to be able to inform:

* Sale number
* Date when the sale was made
* Customer
* Total sale amount
* Branch where the sale was made
* Products
* Quantities
* Unit prices
* Discounts
* Total amount for each item
* Cancelled/Not Cancelled

It's not mandatory, but it would be a differential to build code for publishing events of:
* SaleCreated
* SaleModified
* SaleCancelled
* ItemCancelled

If you write the code, **it's not required** to actually publish to any Message Broker. You can log a message in the application log or however you find most convenient.

### Business Rules

* Purchases above 4 identical items have a 10% discount
* Purchases between 10 and 20 identical items have a 20% discount
* It's not possible to sell above 20 identical items
* Purchases below 4 items cannot have a discount

These business rules define quantity-based discounting tiers and limitations:

1. Discount Tiers:
   - 4+ items: 10% discount
   - 10-20 items: 20% discount

2. Restrictions:
   - Maximum limit: 20 items per product
   - No discounts allowed for quantities below 4 items

## Overview
This section provides a high-level overview of the project and the various skills and competencies it aims to assess for developer candidates. 

See [Overview](/.doc/overview.md)

## Tech Stack
This section lists the key technologies used in the project, including the backend, testing, frontend, and database components. 

See [Tech Stack](/.doc/tech-stack.md)

## Frameworks
This section outlines the frameworks and libraries that are leveraged in the project to enhance development productivity and maintainability. 

See [Frameworks](/.doc/frameworks.md)

<!-- 
## API Structure
This section includes links to the detailed documentation for the different API resources:
- [API General](./docs/general-api.md)
- [Products API](/.doc/products-api.md)
- [Carts API](/.doc/carts-api.md)
- [Users API](/.doc/users-api.md)
- [Auth API](/.doc/auth-api.md)
-->

## Project Structure
This section describes the overall structure and organization of the project files and directories. 

See [Project Structure](/.doc/project-structure.md)

## Implantar o sistema

### Pré-requisitos
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [PostgreSQL](https://www.postgresql.org/) (local ou em contentor)
- Opcional: [Docker Desktop](https://www.docker.com/products/docker-desktop/) para base de dados e/ou API em contentor

### 1) Base de dados PostgreSQL
- **Docker (repositório):** na pasta `Sales`, execute `docker compose up -d ambev.developerevaluation.database`. Credenciais por omissão estão em [`docker-compose.yml`](docker-compose.yml) (`developer_evaluation`, utilizador `developer`, palavra-passe definida no ficheiro).
- **Connection string** alinhada a esse serviço (exemplo):  
  `Host=localhost;Port=5432;Database=developer_evaluation;Username=developer;Password=<a_do_compose>`
- Se a API correr **dentro da mesma rede Docker** que o Postgres, use como host o nome do serviço: `ambev.developerevaluation.database` em vez de `localhost`, e exponha a porta interna `5432`.

### 2) Migrações (schema)
Na pasta `Sales`:

```bash
dotnet ef database update --project src/Ambev.DeveloperEvaluation.ORM --startup-project src/Ambev.DeveloperEvaluation.WebApi
```

*(Requer a ferramenta EF: `dotnet tool install --global dotnet-ef` se ainda não tiver.)*

### 3) Executar a API (desenvolvimento)
```bash
dotnet run --project src/Ambev.DeveloperEvaluation.WebApi/Ambev.DeveloperEvaluation.WebApi.csproj
```
- HTTP: `http://localhost:5119` — Swagger: `/swagger` (apenas com `ASPNETCORE_ENVIRONMENT=Development`).
- HTTPS: ver [`launchSettings.json`](src/Ambev.DeveloperEvaluation.WebApi/Properties/launchSettings.json).

Ajuste `ConnectionStrings:DefaultConnection` e bloco `Jwt` em `appsettings.Development.json` / variáveis de ambiente (`ConnectionStrings__DefaultConnection`, `Jwt__SecretKey`, etc.) conforme o ambiente.

### 4) Imagem Docker da API (contentor só da WebApi)
Na pasta **`Sales`** (contexto de build onde existe a pasta `src/`):

```bash
docker build -f src/Ambev.DeveloperEvaluation.WebApi/Dockerfile -t ambev-developer-evaluation-webapi:latest .
```

Ao correr o contentor, passe a connection string e JWT (ex.: `-e ConnectionStrings__DefaultConnection=...`, `-e Jwt__SecretKey=...`). O [`docker-compose.yml`](docker-compose.yml) inclui um serviço de exemplo `ambev.developerevaluation.webapi`; pode precisar de `depends_on` e da mesma connection string com host do serviço Postgres para um arranque completo automático.

---

## Testar o sistema

### Testes automatizados (CI / local)
Na pasta `Sales`:

| Objetivo | Comando |
|----------|---------|
| Compilar | `dotnet build Sales.sln -c Release` |
| Toda a suíte (unitários + integração HTTP + funcionais health) | `dotnet test Sales.sln -c Release` |
| Cobertura mínima **81%** (linhas em **Domain** + **Application**) | `powershell -File ./scripts/verify-coverage.ps1` ou `dotnet test tests/Ambev.DeveloperEvaluation.Unit/Ambev.DeveloperEvaluation.Unit.csproj -c Release -p:RunCoverageAnalysis=true` |

- **Unitários** — regras de domínio, handlers, segurança JWT, etc. (`tests/Ambev.DeveloperEvaluation.Unit`).
- **Integração** — HTTP real contra a WebApi com SQLite em ficheiro temporário (`tests/Ambev.DeveloperEvaluation.Integration`).
- **Funcionais** — health checks (`/health`, `/health/live`, `/health/ready`) com host de teste (`tests/Ambev.DeveloperEvaluation.Functional`).

### Teste manual rápido (Swagger)
1. Com a API em **Development** e a base migrada, abra `https://localhost:7181/swagger` ou `http://localhost:5119/swagger`.
2. `POST /api/users` — registo (sem JWT).
3. `POST /api/auth` — copie `data.token`.
4. **Authorize** — cole o token (ou `eyJ...` apenas).
5. Experimente `GET /api/sales`, `POST /api/sales`, etc.

### Teste manual com HTTP (exemplo)
```bash
curl -s http://localhost:5119/health/live
```

Com JWT (substitua `<TOKEN>`):
```bash
curl -s -H "Authorization: Bearer <TOKEN>" "http://localhost:5119/api/sales?page=1&pageSize=10"
```

## Local development (backend in this folder)

Resumo: **implantação e testes** detalhados estão nas secções [Implantar o sistema](#implantar-o-sistema) e [Testar o sistema](#testar-o-sistema) acima.

- Abrir e compilar [`Sales.sln`](Sales.sln); código em [`src/`](src/), testes em [`tests/`](tests/).
- PostgreSQL genérico (outro host/credenciais): ver também o bullet em [1) Base de dados](#1-base-de-dados-postgresql) ou `docker run` com imagem `postgres` conforme necessidade local.
- **Autenticação:** JWT via `POST /api/auth` após `POST /api/users`; **paginação** `GET /api/sales`: `page` ≥ 1, `pageSize` 1–10000 (omissão → página 1 e tamanho 1000). Comandos MediatR: `CreateSaleCommand`, `GetSaleCommand`, `ListSalesQuery`, `UpdateSaleCommand`, `DeleteSaleCommand` com `ISaleLineDiscountCalculator`.

**Sales HTTP API** (`SalesController`):

| Method | Route | Description |
|--------|--------|-------------|
| `POST` | `/api/sales` | Create sale (body: sale header + `items[]`; discounts calculated server-side). |
| `GET` | `/api/sales/{id}` | Get sale with line items. |
| `GET` | `/api/sales?page=1&pageSize=1000` | List sales (paginated: `page` ≥ 1, `pageSize` 1–10000; defaults page 1 and page 1000 if query omitted). |
| `PUT` | `/api/sales/{id}` | Update sale (same body shape as create). |
| `DELETE` | `/api/sales/{id}` | Delete sale. |

**Nota:** todos os métodos em `/api/sales` exigem JWT (`Authorization: Bearer`). Obtenha o token com `POST /api/auth` após `POST /api/users`.
