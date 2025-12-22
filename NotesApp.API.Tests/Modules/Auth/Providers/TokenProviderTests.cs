using Microsoft.Extensions.Options;
using NotesApp.API.Modules.Auth.Settings;
using NotesApp.API.Modules.Auth.Utility;
using System.Security.Claims;

namespace NotesApp.API.Tests.Modules.Auth.Providers;

[TestFixture]
public class TokenProviderTests
{
    private JwtSettings _jwtSettings = null!;
    private TokenProvider _tokenProvider = null!;

    [SetUp]
    public void Setup()
    {
        var keyBytes = new byte[64];
        Random.Shared.NextBytes(keyBytes);
        var base64Key = Convert.ToBase64String(keyBytes);

        _jwtSettings = new JwtSettings
        {
            Key = base64Key,
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            ExpiresInHours = 24
        };

        var options = Options.Create(_jwtSettings);
        _tokenProvider = new TokenProvider(options);
    }

    [Test]
    public void CreateToken_ShouldReturnValidJwtToken()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var email = "test@example.com";
        var role = "User";

        // Act
        var token = _tokenProvider.CreateToken(userId, email, role);

        // Assert
        Assert.That(token, Is.Not.Null);
        Assert.That(token, Is.Not.Empty);
    }

    [Test]
    public void CreateToken_ShouldIncludeCorrectClaims()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var email = "test@example.com";
        var role = "Admin";

        // Act
        var token = _tokenProvider.CreateToken(userId, email, role);
        var claims = _tokenProvider.GetClaimsFromToken(token);

        // Assert
        Assert.That(claims, Is.Not.Null);
        Assert.That(claims?.FindFirst("id")?.Value, Is.EqualTo(userId.ToString()));
        Assert.That(claims?.FindFirst(ClaimTypes.Email)?.Value, Is.EqualTo(email));
        Assert.That(claims?.FindFirst(ClaimTypes.Role)?.Value, Is.EqualTo(role));
    }

    [Test]
    public void ValidateToken_WithValidToken_ShouldReturnTrue()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var email = "test@example.com";
        var role = "User";
        var token = _tokenProvider.CreateToken(userId, email, role);

        // Act
        var isValid = _tokenProvider.ValidateToken(token);

        // Assert
        Assert.That(isValid, Is.True);
    }

    [Test]
    public void ValidateToken_WithInvalidToken_ShouldReturnFalse()
    {
        // Arrange
        var invalidToken = "invalid.token.here";

        // Act
        var isValid = _tokenProvider.ValidateToken(invalidToken);

        // Assert
        Assert.That(isValid, Is.False);
    }

    [Test]
    public void GetClaimsFromToken_WithValidToken_ShouldReturnClaimsPrincipal()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var email = "test@example.com";
        var role = "User";
        var token = _tokenProvider.CreateToken(userId, email, role);

        // Act
        var claims = _tokenProvider.GetClaimsFromToken(token);

        // Assert
        Assert.That(claims, Is.Not.Null);
    }

    [Test]
    public void GetClaimsFromToken_WithInvalidToken_ShouldReturnNull()
    {
        // Arrange
        var invalidToken = "invalid.token.here";

        // Act
        var claims = _tokenProvider.GetClaimsFromToken(invalidToken);

        // Assert
        Assert.That(claims, Is.Null);
    }
}
