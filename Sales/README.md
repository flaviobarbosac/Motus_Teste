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

## Local development (backend in this folder)

- Open and build [`Sales.sln`](Sales.sln) at the root of this folder; source is under [`src/`](src/) and tests under [`tests/`](tests/).
- Build: `dotnet build Sales.sln -c Release`
- Tests: `dotnet test Sales.sln -c Release` (inclui unitários, integração HTTP e **funcionais** em `tests/Ambev.DeveloperEvaluation.Functional` — health checks da API).
- **Cobertura mínima (81% linhas em Domain + Application):** na raiz da pasta Sales, execute `powershell -File ./scripts/verify-coverage.ps1` (ou `pwsh` no Linux/macOS) ou `dotnet test tests/Ambev.DeveloperEvaluation.Unit/Ambev.DeveloperEvaluation.Unit.csproj -c Release -p:RunCoverageAnalysis=true` (falha o build se estiver abaixo do limiar).
- PostgreSQL local (exemplo com Docker Desktop):  
  `docker run --name meu-postgres -e POSTGRES_PASSWORD=suasenha -p 5432:5432 -d postgres`  
  Utilizador e base por omissão da imagem: `postgres` / `postgres`. A connection string em `appsettings.json` e `appsettings.Development.json` aponta para `localhost:5432` com essas credenciais. Para outra base, use `-e POSTGRES_DB=nome_da_base` e alinhe `Database=` na connection string.  
  Alternativa do repositório: `docker compose up -d ambev.developerevaluation.database` (credenciais no `docker-compose.yml`).
- Apply EF migrations (PostgreSQL):  
  `dotnet ef database update --project src/Ambev.DeveloperEvaluation.ORM --startup-project src/Ambev.DeveloperEvaluation.WebApi`
- Aplicação Sales (MediatR): `CreateSaleCommand`, `GetSaleCommand`, `ListSalesQuery`, `UpdateSaleCommand`, `DeleteSaleCommand` — totais e descontos por linha via `ISaleLineDiscountCalculator`.
- **Paginação `GET /api/sales`:** `page` ≥ 1; `pageSize` entre **1 e 10000** (validação em `ListSalesQueryRequestValidator`, constantes em `SalesListPagination`). Se omitir a query string, usa-se **página 1** e **pageSize = 1000** por omissão.
- **Autenticação:** `POST /api/users` (registo, anónimo) e `POST /api/auth` (JWT em `data.token`). Os endpoints de **Sales** e a maior parte de **Users** exigem cabeçalho `Authorization: Bearer <token>`; detalhes e exemplos em `/swagger` (Development).
- Run API (Development): `dotnet run --project src/Ambev.DeveloperEvaluation.WebApi/Ambev.DeveloperEvaluation.WebApi.csproj` then open Swagger at `/swagger` (see `launchSettings.json` for ports).

**Sales HTTP API** (`SalesController`):

| Method | Route | Description |
|--------|--------|-------------|
| `POST` | `/api/sales` | Create sale (body: sale header + `items[]`; discounts calculated server-side). |
| `GET` | `/api/sales/{id}` | Get sale with line items. |
| `GET` | `/api/sales?page=1&pageSize=1000` | List sales (paginated: `page` ≥ 1, `pageSize` 1–10000; defaults page 1 and page 1000 if query omitted). |
| `PUT` | `/api/sales/{id}` | Update sale (same body shape as create). |
| `DELETE` | `/api/sales/{id}` | Delete sale. |

**Nota:** todos os métodos em `/api/sales` exigem JWT (`Authorization: Bearer`). Obtenha o token com `POST /api/auth` após `POST /api/users`.
