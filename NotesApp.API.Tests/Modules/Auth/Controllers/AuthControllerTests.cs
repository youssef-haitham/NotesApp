using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NotesApp.API.Common.Dtos;
using NotesApp.API.Common.Exceptions;
using NotesApp.API.Interfaces.Services;
using NotesApp.API.Modules.Auth.Controllers;
using NotesApp.API.Modules.Auth.Dtos.Request;
using NotesApp.API.Modules.Auth.Dtos.Response;
using System.Security.Claims;

namespace NotesApp.API.Tests.Modules.Auth.Controllers;

[TestFixture]
public class AuthControllerTests
{
    private Mock<ILogger<AuthController>> _loggerMock = null!;
    private Mock<IAuthService> _authServiceMock = null!;
    private Mock<IUserService> _userServiceMock = null!;
    private AuthController _authController = null!;

    [SetUp]
    public void Setup()
    {
        _loggerMock = new Mock<ILogger<AuthController>>();
        _authServiceMock = new Mock<IAuthService>();
        _userServiceMock = new Mock<IUserService>();

        _authController = new AuthController(
            _loggerMock.Object,
            _authServiceMock.Object,
            _userServiceMock.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };
    }

    [Test]
    public async Task SignUp_ShouldReturnOkWithAuthResponse()
    {
        // Arrange
        var signUpRequest = new SignUpRequestDto
        {
            Name = "Test User",
            Email = "test@example.com",
            Password = "Password123"
        };

        var authResponse = new AuthResponseDto
        {
            Id = Guid.NewGuid(),
            Email = signUpRequest.Email,
            Name = signUpRequest.Name
        };

        _authServiceMock.Setup(x => x.SignUp(signUpRequest))
            .ReturnsAsync((authResponse, "test_token"));

        // Act
        var result = await _authController.SignUp(signUpRequest);

        // Assert
        Assert.That(result, Is.InstanceOf<OkObjectResult>());
        var okResult = result as OkObjectResult;
        Assert.That(okResult?.Value, Is.EqualTo(authResponse));
    }

    [Test]
    public void SignUp_WhenUserExists_ShouldThrowConflictException()
    {
        // Arrange
        var signUpRequest = new SignUpRequestDto
        {
            Name = "Test User",
            Email = "test@example.com",
            Password = "Password123"
        };

        _authServiceMock.Setup(x => x.SignUp(signUpRequest))
            .ThrowsAsync(new ConflictException("User", "email", signUpRequest.Email));

        // Act & Assert
        var ex = Assert.ThrowsAsync<ConflictException>(async () =>
            await _authController.SignUp(signUpRequest));
        Assert.That(ex, Is.Not.Null);
    }

    [Test]
    public async Task SignIn_ShouldReturnOkWithAuthResponse()
    {
        // Arrange
        var signInRequest = new SignInRequestDto
        {
            Email = "test@example.com",
            Password = "Password123"
        };

        var authResponse = new AuthResponseDto
        {
            Id = Guid.NewGuid(),
            Email = signInRequest.Email,
            Name = "Test User"
        };

        _authServiceMock.Setup(x => x.SignIn(signInRequest))
            .ReturnsAsync((authResponse, "test_token"));

        // Act
        var result = await _authController.SignIn(signInRequest);

        // Assert
        Assert.That(result, Is.InstanceOf<OkObjectResult>());
        var okResult = result as OkObjectResult;
        Assert.That(okResult?.Value, Is.EqualTo(authResponse));
    }

    [Test]
    public void SignIn_WhenCredentialsInvalid_ShouldThrowBadRequestException()
    {
        // Arrange
        var signInRequest = new SignInRequestDto
        {
            Email = "test@example.com",
            Password = "WrongPassword"
        };

        _authServiceMock.Setup(x => x.SignIn(signInRequest))
            .ThrowsAsync(new BadRequestException("Email or Password are incorrect"));

        // Act & Assert
        var ex = Assert.ThrowsAsync<BadRequestException>(async () =>
            await _authController.SignIn(signInRequest));
        Assert.That(ex, Is.Not.Null);
    }

    [Test]
    public async Task Me_WhenUserExists_ShouldReturnOk()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userDto = new UserDto
        {
            Id = userId,
            Email = "test@example.com",
            Name = "Test User"
        };

        var claims = new List<Claim>
        {
            new Claim("id", userId.ToString())
        };
        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);

        _authController.ControllerContext.HttpContext.User = principal;
        _userServiceMock.Setup(x => x.GetUserById(userId))
            .ReturnsAsync(userDto);

        // Act
        var result = await _authController.Me();

        // Assert
        Assert.That(result, Is.InstanceOf<OkObjectResult>());
        var okResult = result as OkObjectResult;
        Assert.That(okResult?.Value, Is.EqualTo(userDto));
    }

    [Test]
    public async Task Me_WhenUserIdNotFound_ShouldReturnUnauthorized()
    {
        // Arrange
        var claims = new List<Claim>();
        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);

        _authController.ControllerContext.HttpContext.User = principal;

        // Act
        var result = await _authController.Me();

        // Assert
        Assert.That(result, Is.InstanceOf<UnauthorizedResult>());
    }

    [Test]
    public void Logout_ShouldReturnOkAndDeleteCookie()
    {
        // Act
        var result = _authController.Logout();

        // Assert
        Assert.That(result, Is.InstanceOf<OkObjectResult>());
    }

    [Test]
    public async Task GetAllUsers_ShouldReturnOkWithPagedResponse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var claims = new List<Claim>
        {
            new Claim("id", userId.ToString()),
            new Claim(ClaimTypes.Role, "Admin")
        };
        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);

        _authController.ControllerContext.HttpContext.User = principal;

        var pagedResponse = new PagedResponseDto<UserDto>
        {
            Data = new List<UserDto>
            {
                new UserDto { Id = Guid.NewGuid(), Email = "user1@example.com", Name = "User 1" }
            },
            PageNumber = 1,
            PageSize = 10,
            TotalCount = 1
        };

        _userServiceMock.Setup(x => x.GetUsersAsync(1, 10))
            .ReturnsAsync(pagedResponse);

        // Act
        var result = await _authController.GetAllUsers(1, 10);

        // Assert
        Assert.That(result, Is.InstanceOf<OkObjectResult>());
        var okResult = result as OkObjectResult;
        Assert.That(okResult?.Value, Is.EqualTo(pagedResponse));
    }
}

