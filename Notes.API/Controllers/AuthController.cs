using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NotesApp.API.Dtos.Request;
using NotesApp.API.Dtos.Response;
using NotesApp.API.Interfaces.Services;

namespace NotesApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController(IAuthService authService, IUserService userService) : ControllerBase
    {
        private readonly IAuthService _authService = authService;
        private readonly IUserService _userService = userService;

        [HttpPost("signup")]
        public async Task<IActionResult> SignUp([FromBody] SignUpRequestDto user)
        {
            try
            {
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

                return Ok(authResponse);
            }
            catch (Exception ex)
            {
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

                return Ok(authResponse);
            }
            catch(Exception ex)
            {
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
            string? userId = User.FindFirst("id")?.Value;
            if (userId is null)
                return Unauthorized();

            UserDto? userDto = await _userService.GetUserById(Guid.Parse(userId));
            if (userDto == null)
            {
                Response.Cookies.Delete("auth_token");
                return Unauthorized();
            }

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
