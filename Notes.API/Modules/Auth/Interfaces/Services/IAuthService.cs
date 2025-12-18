using NotesApp.API.Modules.Auth.Dtos.Request;
using NotesApp.API.Modules.Auth.Dtos.Response;

namespace NotesApp.API.Interfaces.Services
{
    public interface IAuthService
    {
        Task<(AuthResponseDto authResponse, string token)> SignUp(SignUpRequestDto user);
        Task<(AuthResponseDto authResponse, string token)> SignIn(SignInRequestDto user);
    }
}