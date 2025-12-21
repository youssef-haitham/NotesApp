using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NotesApp.API.Interfaces.Services;
using NotesApp.API.Modules.Auth.Dtos.Request;

namespace NotesApp.API.Modules.Auth.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController(ILogger<AuthController> logger, IAuthService authService, IUserService userService) : ControllerBase
    {
        private readonly ILogger<AuthController> _logger = logger;
        private readonly IAuthService _authService = authService;
        private readonly IUserService _userService = userService;

        [HttpPost("signup")]
        public async Task<IActionResult> SignUp([FromBody] SignUpRequestDto user)
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

        [HttpPost("signin")]
        public async Task<IActionResult> SignIn([FromBody] SignInRequestDto user)
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
            };
            Response.Cookies.Append("auth_token", token, cookieOptions);

            _logger.LogInformation("SignIn: Signedin successfully for email {}", user.Email);
            return Ok(authResponse);
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
                _logger.LogWarning("Me: Failed to find ID for Me Request - token may be invalid");
                Response.Cookies.Delete("auth_token");
                return Unauthorized();
            }

            var userDto = await _userService.GetUserById(Guid.Parse(userId));

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

        [Authorize(Policy = "AdminOnly")]
        [HttpGet("admin/users")]
        public async Task<IActionResult> GetAllUsers([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            _logger.LogInformation("GetAllUsers: Admin requested users - Page {PageNumber}, Size {PageSize}", pageNumber, pageSize);

            var result = await _userService.GetUsersAsync(pageNumber, pageSize);

            _logger.LogInformation("GetAllUsers: Retrieved {Count} users out of {Total}", result.Data.Count(), result.TotalCount);
            return Ok(result);
        }
    }
}
