using Ambev.DeveloperEvaluation.Application.Sales.CreateSale;
using Ambev.DeveloperEvaluation.Application.Sales.DeleteSale;
using Ambev.DeveloperEvaluation.Application.Sales.GetSale;
using Ambev.DeveloperEvaluation.Application.Sales.ListSales;
using Ambev.DeveloperEvaluation.Application.Sales.UpdateSale;
using Ambev.DeveloperEvaluation.Domain.Exceptions;
using Ambev.DeveloperEvaluation.WebApi.Common;
using Ambev.DeveloperEvaluation.WebApi.Features.Sales.CreateSale;
using Ambev.DeveloperEvaluation.WebApi.Features.Sales.DeleteSale;
using Ambev.DeveloperEvaluation.WebApi.Features.Sales.GetSale;
using Ambev.DeveloperEvaluation.WebApi.Features.Sales.ListSales;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Sales;

/// <summary>
/// API REST de vendas: criação, consulta, listagem paginada, atualização e eliminação.
/// </summary>
/// <remarks>
/// Requer JWT válido (exceto fluxo de autenticação noutros controladores). Descontos por linha e totais são calculados no servidor.
/// </remarks>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SalesController : BaseController
{
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;

    public SalesController(IMediator mediator, IMapper mapper)
    {
        _mediator = mediator;
        _mapper = mapper;
    }

    /// <summary>
    /// Cria uma nova venda com linhas; descontos e total são calculados no servidor.
    /// </summary>
    /// <param name="request">Cabeçalho da venda e linhas (produto, quantidade, preço unitário).</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>201 com identificador e total; 400 regra de negócio/validação; 409 número de venda duplicado.</returns>
    /// <response code="401">JWT em falta ou inválido.</response>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponseWithData<CreateSaleResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateSaleRequest request, CancellationToken cancellationToken)
    {
        var validator = new CreateSaleRequestValidator();
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            return BadRequest(validationResult.Errors);

        try
        {
            var command = _mapper.Map<CreateSaleCommand>(request);
            var result = await _mediator.Send(command, cancellationToken);
            return Created(string.Empty, new ApiResponseWithData<CreateSaleResponse>
            {
                Success = true,
                Message = "Sale created successfully",
                Data = _mapper.Map<CreateSaleResponse>(result)
            });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new ApiResponse { Success = false, Message = ex.Message });
        }
        catch (DomainException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Obtém uma venda pelo identificador, incluindo todas as linhas e totais.
    /// </summary>
    /// <param name="id">Identificador único (GUID) da venda.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>200 com dados da venda; 404 se não existir.</returns>
    /// <response code="401">JWT em falta ou inválido.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponseWithData<GetSaleResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var routeRequest = new GetSaleRequest { Id = id };
        var validator = new GetSaleRequestValidator();
        var validationResult = await validator.ValidateAsync(routeRequest, cancellationToken);
        if (!validationResult.IsValid)
            return BadRequest(validationResult.Errors);

        try
        {
            var result = await _mediator.Send(new GetSaleCommand(id), cancellationToken);
            return OkRaw(new ApiResponseWithData<GetSaleResponse>
            {
                Success = true,
                Message = "Sale retrieved successfully",
                Data = _mapper.Map<GetSaleResponse>(result)
            });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    /// <summary>
    /// Lista vendas com paginação. Sem query string usa página 1 e tamanho de página por omissão do sistema; existe limite máximo por pedido.
    /// </summary>
    /// <param name="query">Parâmetros <c>page</c> (≥ 1) e <c>pageSize</c> (intervalo permitido pela API).</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>200 com <c>data</c>, <c>totalCount</c>, <c>currentPage</c> e <c>totalPages</c>.</returns>
    /// <response code="401">JWT em falta ou inválido.</response>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResponse<GetSaleResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> List([FromQuery] ListSalesQueryRequest query, CancellationToken cancellationToken)
    {
        var validator = new ListSalesQueryRequestValidator();
        var validationResult = await validator.ValidateAsync(query, cancellationToken);
        if (!validationResult.IsValid)
            return BadRequest(validationResult.Errors);

        var result = await _mediator.Send(new ListSalesQuery { Page = query.Page, PageSize = query.PageSize }, cancellationToken);
        var items = _mapper.Map<List<GetSaleResponse>>(result.Items);
        var paged = new PaginatedList<GetSaleResponse>(items, result.TotalCount, result.Page, result.PageSize);
        return OkPaginated(paged);
    }

    /// <summary>
    /// Atualiza uma venda existente (substitui linhas; descontos e totais recalculados).
    /// </summary>
    /// <param name="id">Identificador da venda a atualizar.</param>
    /// <param name="request">Novo estado da venda (mesmo formato que na criação).</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>200 com venda atualizada; 404 não encontrada; 409 conflito de negócio.</returns>
    /// <response code="401">JWT em falta ou inválido.</response>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponseWithData<GetSaleResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] CreateSaleRequest request, CancellationToken cancellationToken)
    {
        var validator = new CreateSaleRequestValidator();
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            return BadRequest(validationResult.Errors);

        var command = _mapper.Map<UpdateSaleCommand>(request);
        command.Id = id;

        try
        {
            var result = await _mediator.Send(command, cancellationToken);
            return OkRaw(new ApiResponseWithData<GetSaleResponse>
            {
                Success = true,
                Message = "Sale updated successfully",
                Data = _mapper.Map<GetSaleResponse>(result)
            });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new ApiResponse { Success = false, Message = ex.Message });
        }
        catch (DomainException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Elimina uma venda e as respetivas linhas.
    /// </summary>
    /// <param name="id">Identificador da venda a eliminar.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>200 em sucesso; 404 se a venda não existir.</returns>
    /// <response code="401">JWT em falta ou inválido.</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var routeRequest = new DeleteSaleRequest { Id = id };
        var validator = new DeleteSaleRequestValidator();
        var validationResult = await validator.ValidateAsync(routeRequest, cancellationToken);
        if (!validationResult.IsValid)
            return BadRequest(validationResult.Errors);

        try
        {
            await _mediator.Send(new DeleteSaleCommand(id), cancellationToken);
            return OkRaw(new ApiResponse
            {
                Success = true,
                Message = "Sale deleted successfully"
            });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }
}
