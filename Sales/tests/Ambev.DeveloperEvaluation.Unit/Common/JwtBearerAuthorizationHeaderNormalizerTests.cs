using Ambev.DeveloperEvaluation.Common.Security;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Common;

public class JwtBearerAuthorizationHeaderNormalizerTests
{
    [Theory]
    [InlineData("Bearer eyJ.h.sig", "eyJ.h.sig")]
    [InlineData("bearer eyJ.h.sig", "eyJ.h.sig")]
    [InlineData("Bearer bearer eyJ.h.sig", "eyJ.h.sig")]
    [InlineData("Bearer BEARER eyJ.h.sig", "eyJ.h.sig")]
    [InlineData("  Bearer   Bearer   eyJ.h.sig  ", "eyJ.h.sig")]
    [InlineData("\"eyJ.h.sig\"", "eyJ.h.sig")]
    [InlineData("Bearer \"eyJ.h.sig\"", "eyJ.h.sig")]
    public void ExtractJwt_strips_bearer_prefixes_and_quotes(string input, string expected) =>
        JwtBearerAuthorizationHeaderNormalizer.ExtractJwt(input).Should().Be(expected);

    [Fact]
    public void ExtractJwt_returns_null_for_two_segments() =>
        JwtBearerAuthorizationHeaderNormalizer.ExtractJwt("a.b").Should().BeNull();

    [Fact]
    public void ExtractJwt_returns_null_for_empty() =>
        JwtBearerAuthorizationHeaderNormalizer.ExtractJwt("   ").Should().BeNull();
}
