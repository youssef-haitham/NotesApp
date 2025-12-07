using Microsoft.EntityFrameworkCore;
using NotesApp.API.DBContext;
using NotesApp.API.Interfaces.Repositories;
using NotesApp.API.Models;

namespace NotesApp.API.Repositories
{
    public class UserRepository(NoteDBContext noteDBContext) : IUserRepository
    {
        private readonly NoteDBContext _noteDBContext = noteDBContext;

        public async Task<User> AddUserAsync(User user)
        {
            var addedUser = await _noteDBContext.User.AddAsync(user);
            await _noteDBContext.SaveChangesAsync();

            return addedUser.Entity;
        }

        public Task<bool> DeleteUserByIdAsync(Guid userId)
        {
            throw new NotImplementedException();
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await _noteDBContext.User.FirstOrDefaultAsync(u => u.Email.Equals(email));
        }

        public async Task<User?> GetUserByIdAsync(Guid userId)
        {
            return await _noteDBContext.User.FirstOrDefaultAsync(u => u.Id == userId);
        }

        public Task<IEnumerable<User>> GetUsersAsync(int pageNumber, int pageSize)
        {
            throw new NotImplementedException();
        }

        public Task<User?> UpdateUserAsync(User user)
        {
            throw new NotImplementedException();
        }

        public Task<bool> UserExistsAsync(Guid userId)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> UserExistsByEmailAsync(string email)
        {
            return await _noteDBContext.User.AnyAsync(u => u.Email.Equals(email));
        }
    }
}
