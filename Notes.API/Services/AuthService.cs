using NotesApp.API.Dtos.Request;
using NotesApp.API.Dtos.Response;
using NotesApp.API.Interfaces.Repositories;
using NotesApp.API.Interfaces.Services;
using NotesApp.API.Interfaces.Utility;
using NotesApp.API.Models;
using NotesApp.API.Utility;

namespace NotesApp.API.Services
{
    public class AuthService(IUserRepository userRepo, ITokenHelper tokenHelper) : IAuthService
    {
        private readonly IUserRepository _userRepo = userRepo;
        private readonly ITokenHelper _tokenHelper = tokenHelper;

        public async Task<(AuthResponseDto authResponse, string token)> SignUp(SignUpRequestDto user)
        {
            var hashedPassword = HashHelper.HashPassword(user.Password);

            if (await _userRepo.UserExistsByEmailAsync(user.Email))
            {
                throw new Exception("Email already exists");
            }
            var newuser = new User()
            {
                Email = user.Email,
                Name = user.Name,
                PasswordHash = hashedPassword
            };
            var addedUser = await _userRepo.AddUserAsync(newuser);
            var token = _tokenHelper.CreateToken(addedUser.Id, addedUser.Email);

            return (new AuthResponseDto() { Id = addedUser.Id, Email = addedUser.Email, Name = addedUser.Name}, token);
        }

        public async Task<(AuthResponseDto authResponse, string token)> SignIn(SignInRequestDto user)
        {
            var userExist = await _userRepo.GetUserByEmailAsync(user.Email) ?? throw new BadHttpRequestException("Email or Password are incorrect");

            var passwordVerified = HashHelper.Verify(user.Password, userExist.PasswordHash);
            if(!passwordVerified) throw new BadHttpRequestException("Email or Password are incorrect");

            var token = _tokenHelper.CreateToken(userExist.Id, userExist.Email);
            return (new AuthResponseDto() { Id = userExist.Id, Email = userExist.Email, Name = userExist.Name }, token);
        }
    }
}