using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using Ambev.DeveloperEvaluation.WebApi.Common;
using Ambev.DeveloperEvaluation.WebApi.Features.Users.CreateUser;
using Ambev.DeveloperEvaluation.WebApi.Features.Users.GetUser;
using Ambev.DeveloperEvaluation.WebApi.Features.Users.DeleteUser;
using Ambev.DeveloperEvaluation.WebApi.Features.Users.ListUsers;
using Ambev.DeveloperEvaluation.Application.Users.CreateUser;
using Ambev.DeveloperEvaluation.Application.Users.GetUser;
using Ambev.DeveloperEvaluation.Application.Users.DeleteUser;
using Ambev.DeveloperEvaluation.Application.Users.ListUsers;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Users;

/// <summary>
/// Gestão de utilizadores: registo, listagem, consulta e eliminação.
/// </summary>
/// <remarks>
/// <c>POST /api/users</c> é anónimo (registo). Os restantes métodos exigem JWT.
/// </remarks>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : BaseController
{
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;

    /// <summary>
    /// Inicializa o controlador de utilizadores.
    /// </summary>
    /// <param name="mediator">MediatR para comandos e consultas.</param>
    /// <param name="mapper">Mapeamento entre modelos API e aplicação.</param>
    public UsersController(IMediator mediator, IMapper mapper)
    {
        _mediator = mediator;
        _mapper = mapper;
    }

    /// <summary>
    /// Regista um novo utilizador (público; não envie cabeçalho Authorization).
    /// </summary>
    /// <param name="request">Nome de utilizador, email, telefone, palavra-passe, estado e perfil.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>201 com dados do utilizador criado (sem palavra-passe).</returns>
    [HttpPost]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponseWithData<CreateUserResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request, CancellationToken cancellationToken)
    {
        var validator = new CreateUserRequestValidator();
        var validationResult = await validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
            return BadRequest(validationResult.Errors);

        var command = _mapper.Map<CreateUserCommand>(request);
        var response = await _mediator.Send(command, cancellationToken);

        return Created(string.Empty, new ApiResponseWithData<CreateUserResponse>
        {
            Success = true,
            Message = "User created successfully",
            Data = _mapper.Map<CreateUserResponse>(response)
        });
    }

    /// <summary>
    /// Lista utilizadores com paginação (tamanho de página limitado pela API).
    /// </summary>
    /// <param name="query">Página e tamanho da página.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>200 com lista paginada.</returns>
    /// <response code="401">JWT em falta ou inválido.</response>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResponse<GetUserResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ListUsers([FromQuery] ListUsersQueryRequest query, CancellationToken cancellationToken)
    {
        var validator = new ListUsersQueryRequestValidator();
        var validationResult = await validator.ValidateAsync(query, cancellationToken);
        if (!validationResult.IsValid)
            return BadRequest(validationResult.Errors);

        var result = await _mediator.Send(new ListUsersQuery { Page = query.Page, PageSize = query.PageSize }, cancellationToken);
        var items = _mapper.Map<List<GetUserResponse>>(result.Items);
        var paged = new PaginatedList<GetUserResponse>(items, result.TotalCount, result.Page, result.PageSize);
        return OkPaginated(paged);
    }

    /// <summary>
    /// Obtém os dados de um utilizador pelo identificador.
    /// </summary>
    /// <param name="id">GUID do utilizador.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>200 com perfil; 404 se não existir.</returns>
    /// <response code="401">JWT em falta ou inválido.</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponseWithData<GetUserResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUser([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var request = new GetUserRequest { Id = id };
        var validator = new GetUserRequestValidator();
        var validationResult = await validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
            return BadRequest(validationResult.Errors);

        var command = _mapper.Map<GetUserCommand>(request.Id);
        var response = await _mediator.Send(command, cancellationToken);

        return OkRaw(new ApiResponseWithData<GetUserResponse>
        {
            Success = true,
            Message = "User retrieved successfully",
            Data = _mapper.Map<GetUserResponse>(response)
        });
    }

    /// <summary>
    /// Elimina um utilizador pelo identificador.
    /// </summary>
    /// <param name="id">GUID do utilizador a eliminar.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>200 em sucesso; 404 se não existir.</returns>
    /// <response code="401">JWT em falta ou inválido.</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteUser([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var request = new DeleteUserRequest { Id = id };
        var validator = new DeleteUserRequestValidator();
        var validationResult = await validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
            return BadRequest(validationResult.Errors);

        var command = _mapper.Map<DeleteUserCommand>(request.Id);
        await _mediator.Send(command, cancellationToken);

        return OkRaw(new ApiResponse
        {
            Success = true,
            Message = "User deleted successfully"
        });
    }
}
