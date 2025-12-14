using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NotesApp.API.Dtos.Request;
using NotesApp.API.Dtos.Response;
using NotesApp.API.Interfaces.Services;

namespace NotesApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController(ILogger<AuthController> logger ,IAuthService authService, IUserService userService) : ControllerBase
    {
        private readonly ILogger<AuthController> _logger = logger;
        private readonly IAuthService _authService = authService;
        private readonly IUserService _userService = userService;

        [HttpPost("signup")]
        public async Task<IActionResult> SignUp([FromBody] SignUpRequestDto user)
        {
            try
            {
                _logger.LogInformation("SignUp: Received Signup request for email {}", user.Email);
                var (authResponse, token) = await _authService.SignUp(user);

                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.None,
                    Expires = DateTime.UtcNow.AddHours(24),
                    Path = "/"
                };
                Response.Cookies.Append("auth_token", token, cookieOptions);

                _logger.LogInformation("SignUp: User Signedup successfully for email {}", user.Email);

                return Ok(authResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SignUp: Failed to signup user {}", user.Email);
                return Problem(
                    detail: ex.Message,
                    title: "Something went wrong",
                    statusCode: StatusCodes.Status500InternalServerError
                );
            }
        }

        [HttpPost("signin")]
        public async Task<IActionResult> SignIn([FromBody] SignInRequestDto user)
        {
            try
            {
                _logger.LogInformation("SignIn: Received Signin request for user {}", user.Email);
                var (authResponse, token) = await _authService.SignIn(user);

                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.None,
                    Expires = DateTime.UtcNow.AddHours(24),
                    Path = "/"
                }; ;
                Response.Cookies.Append("auth_token", token, cookieOptions);

                _logger.LogInformation("SignIn: Signedin successfully for email {}", user.Email);
                return Ok(authResponse);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "SignIn: Failed to signin user {}", user.Email);
                return Problem(
                    detail: ex.Message,
                    title: "Something went wrong",
                    statusCode: StatusCodes.Status500InternalServerError
                );
            }
        }

        [Authorize]
        [HttpGet]
        [Route("me")]
        public async Task<IActionResult> Me()
        {
            _logger.LogInformation("Me: Received Me request");
            string? userId = User.FindFirst("id")?.Value;

            if (userId is null)
            {
                _logger.LogError("Me: Failed to find ID for Me Request");
                return Unauthorized();
            }

            UserDto? userDto = await _userService.GetUserById(Guid.Parse(userId));
            if (userDto == null)
            {
                Response.Cookies.Delete("auth_token");
                return Unauthorized();
            }

            _logger.LogInformation("Me: User returned successfully");
            return Ok(userDto);
        }

        [Authorize]
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            Response.Cookies.Delete("auth_token");

            return Ok(new { message = "Logged out successfully" });
        }
    }
}
