using Microsoft.EntityFrameworkCore;
using NotesApp.API.Infrastructure.DBContext;
using NotesApp.API.Infrastructure.Models;
using NotesApp.API.Interfaces.Repositories;

namespace NotesApp.API.Modules.Auth.Repositories
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
            return await _noteDBContext.User
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Email.Equals(email));
        }

        public async Task<User?> GetUserByIdAsync(Guid userId)
        {
            return await _noteDBContext.User
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Id == userId);
        }

        public async Task<(IEnumerable<User> users, int totalCount)> GetUsersAsync(int pageNumber, int pageSize)
        {
            var query = _noteDBContext.User
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .AsQueryable();

            var totalCount = await query.CountAsync();

            var users = await query
                .OrderByDescending(u => u.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (users, totalCount);
        }

        public async Task<User?> UpdateUserAsync(User user)
        {
            _noteDBContext.User.Update(user);
            await _noteDBContext.SaveChangesAsync();
            return user;
        }

        public Task<bool> UserExistsAsync(Guid userId)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> UserExistsByEmailAsync(string email)
        {
            return await _noteDBContext.User.AnyAsync(u => u.Email.Equals(email));
        }

        public async Task<UserRole> AddUserRoleAsync(UserRole userRole)
        {
            var addedUserRole = await _noteDBContext.UserRole.AddAsync(userRole);
            await _noteDBContext.SaveChangesAsync();
            return addedUserRole.Entity;
        }
    }
}
