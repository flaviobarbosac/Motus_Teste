using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using Ambev.DeveloperEvaluation.WebApi.Common;
using Ambev.DeveloperEvaluation.WebApi.Features.Auth.AuthenticateUserFeature;
using Ambev.DeveloperEvaluation.Application.Auth.AuthenticateUser;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Auth;

/// <summary>
/// Autenticação: emissão de JWT para sessões autenticadas.
/// </summary>
/// <remarks>
/// Não requer <c>Authorization</c>. Utilize o token devolvido nas restantes operações protegidas.
/// </remarks>
[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public class AuthController : BaseController
{
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;

    /// <summary>
    /// Inicializa o controlador de autenticação.
    /// </summary>
    /// <param name="mediator">MediatR.</param>
    /// <param name="mapper">Mapeamento API ↔ aplicação.</param>
    public AuthController(IMediator mediator, IMapper mapper)
    {
        _mediator = mediator;
        _mapper = mapper;
    }

    /// <summary>
    /// Autentica um utilizador ativo com email e palavra-passe e devolve um JWT.
    /// </summary>
    /// <param name="request">Email e palavra-passe.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>200 com <c>data.token</c> (JWT), email, nome e perfil; 401 credenciais inválidas ou conta inativa.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponseWithData<AuthenticateUserResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> AuthenticateUser([FromBody] AuthenticateUserRequest request, CancellationToken cancellationToken)
    {
        var validator = new AuthenticateUserRequestValidator();
        var validationResult = await validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
            return BadRequest(validationResult.Errors);

        var command = _mapper.Map<AuthenticateUserCommand>(request);
        var response = await _mediator.Send(command, cancellationToken);

        return OkRaw(new ApiResponseWithData<AuthenticateUserResponse>
        {
            Success = true,
            Message = "User authenticated successfully",
            Data = _mapper.Map<AuthenticateUserResponse>(response)
        });
    }
}
