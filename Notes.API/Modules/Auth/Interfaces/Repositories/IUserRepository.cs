using NotesApp.API.Infrastructure.Models;

namespace NotesApp.API.Interfaces.Repositories
{
    public interface IUserRepository
    {
        Task<User> AddUserAsync(User user);
        Task<User?> UpdateUserAsync(User user);
        Task<User?> GetUserByIdAsync(Guid userId);
        Task<User?> GetUserByEmailAsync(string email);
        Task<IEnumerable<User>> GetUsersAsync(int pageNumber, int pageSize);
        Task<bool> DeleteUserByIdAsync(Guid userId);
        Task<bool> UserExistsAsync(Guid userId);
        Task<bool> UserExistsByEmailAsync(string email);
    }
}