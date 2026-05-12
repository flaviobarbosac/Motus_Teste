using Ambev.DeveloperEvaluation.Common.Security;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Enums;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Common;

/// <summary>
/// Garante que o token gerado é aceite pelo mesmo validador que o JwtBearer (issuer, audience, HS256).
/// </summary>
public class JwtTokenGeneratorRoundTripTests
{
    private const string Secret =
        "YourSuperSecretKeyForJwtTokenGenerationThatShouldBeAtLeast32BytesLong";

    private static JwtOptions TestOptions() =>
        new()
        {
            SecretKey = Secret,
            Issuer = "test-issuer",
            Audience = "test-audience",
            AccessTokenLifetimeHours = 8
        };

    [Fact]
    public async Task Generated_token_validates_with_JsonWebTokenHandler_same_as_jwt_bearer()
    {
        var opts = TestOptions();
        var signingKey = JwtTokenParametersFactory.CreateSigningKey(opts.SecretKey);
        var validation = JwtTokenParametersFactory.CreateValidationParameters(opts, signingKey);

        IUser user = new User
        {
            Id = Guid.Parse("a1111111-1111-1111-1111-111111111111"),
            Username = "tester",
            Email = "tester@local.test",
            Phone = "+5511987654321",
            Password = "unused",
            Role = UserRole.Customer,
            Status = UserStatus.Active
        };

        var generator = new JwtTokenGenerator(Options.Create(opts));
        var jwt = generator.GenerateToken(user);
        jwt.Should().NotBeNullOrWhiteSpace();
        jwt.Split('.').Should().HaveCount(3, "JWT compacto deve ter 3 segmentos");

        var handler = new JsonWebTokenHandler();
        var result = await handler.ValidateTokenAsync(jwt, validation);

        result.IsValid.Should().BeTrue($"token inválido: {result.Exception?.Message}");
        result.ClaimsIdentity.Should().NotBeNull();
        result.ClaimsIdentity!.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
            .Should().Be(user.Id);
    }
}
