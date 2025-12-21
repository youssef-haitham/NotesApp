using NotesApp.API.Common.Exceptions;
using NotesApp.API.Infrastructure.Models;
using NotesApp.API.Interfaces.Repositories;
using NotesApp.API.Interfaces.Services;
using NotesApp.API.Interfaces.Utility;
using NotesApp.API.Modules.Auth.Dtos.Request;
using NotesApp.API.Modules.Auth.Dtos.Response;
using NotesApp.API.Modules.Auth.Interfaces.Repositories;

namespace NotesApp.API.Modules.Auth.Services
{
    public class AuthService(
        IUserRepository userRepo, 
        IRoleRepository roleRepo,
        ITokenProvider tokenHelper, 
        IHashProvider hashProvider) : IAuthService
    {
        private readonly IUserRepository _userRepo = userRepo;
        private readonly IRoleRepository _roleRepo = roleRepo;
        private readonly ITokenProvider _tokenHelper = tokenHelper;
        private readonly IHashProvider _hashProvider = hashProvider;

        public async Task<(AuthResponseDto authResponse, string token)> SignUp(SignUpRequestDto user)
        {
            var hashedPassword = _hashProvider.HashPassword(user.Password);

            if (await _userRepo.UserExistsByEmailAsync(user.Email))
            {
                throw new ConflictException("User", "email", user.Email);
            }

            var userRole = await _roleRepo.GetRoleByNameAsync("User");
            if (userRole == null)
            {
                throw new Exception("User role not found in database. Please seed roles first.");
            }

            var newuser = new User()
            {
                Email = user.Email,
                Name = user.Name,
                PasswordHash = hashedPassword
            };
            var addedUser = await _userRepo.AddUserAsync(newuser);

            var userRoleEntity = new UserRole
            {
                UserId = addedUser.Id,
                RoleId = userRole.Id,
                User = addedUser,
                Role = userRole
            };
            await _userRepo.AddUserRoleAsync(userRoleEntity);

            var userWithRoles = await _userRepo.GetUserByIdAsync(addedUser.Id);
            var primaryRole = userWithRoles?.UserRoles.FirstOrDefault()?.Role?.Name ?? "User";
            var token = _tokenHelper.CreateToken(addedUser.Id, addedUser.Email, primaryRole);

            return (new AuthResponseDto() { Id = addedUser.Id, Email = addedUser.Email, Name = addedUser.Name}, token);
        }

        public async Task<(AuthResponseDto authResponse, string token)> SignIn(SignInRequestDto user)
        {
            var userExist = await _userRepo.GetUserByEmailAsync(user.Email);
            
            if (userExist == null)
            {
                throw new BadRequestException("Email or Password are incorrect");
            }

            var passwordVerified = _hashProvider.Verify(user.Password, userExist.PasswordHash);
            if (!passwordVerified)
            {
                throw new BadRequestException("Email or Password are incorrect");
            }

            var primaryRole = userExist.UserRoles.FirstOrDefault()?.Role?.Name ?? "User";
            var token = _tokenHelper.CreateToken(userExist.Id, userExist.Email, primaryRole);
            return (new AuthResponseDto() { Id = userExist.Id, Email = userExist.Email, Name = userExist.Name }, token);
        }
    }
}