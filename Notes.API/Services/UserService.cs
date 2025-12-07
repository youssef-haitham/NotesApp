using NotesApp.API.Dtos.Response;
using NotesApp.API.Interfaces.Repositories;
using NotesApp.API.Interfaces.Services;
using NotesApp.API.Models;

namespace NotesApp.API.Services
{
    public class UserService(IUserRepository userRepo) : IUserService
    {
        private readonly IUserRepository _userRepo = userRepo;
        public async Task<UserDto?> GetUserById(Guid id)
        {
            User? user = await _userRepo.GetUserByIdAsync(id);
            return user == null? null : new UserDto { Id = user.Id, Email = user.Email, Name = user.Name };
        }
    }
}