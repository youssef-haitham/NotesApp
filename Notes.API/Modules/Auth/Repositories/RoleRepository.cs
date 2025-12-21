using Microsoft.EntityFrameworkCore;
using NotesApp.API.Infrastructure.DBContext;
using NotesApp.API.Infrastructure.Models;
using NotesApp.API.Modules.Auth.Interfaces.Repositories;

namespace NotesApp.API.Modules.Auth.Repositories
{
    public class RoleRepository(NoteDBContext noteDBContext) : IRoleRepository
    {
        private readonly NoteDBContext _noteDBContext = noteDBContext;

        public async Task<Role?> GetRoleByNameAsync(string roleName)
        {
            return await _noteDBContext.Role
                .FirstOrDefaultAsync(r => r.Name.ToLower() == roleName.ToLower());
        }

        public async Task<Role?> GetRoleByIdAsync(int roleId)
        {
            return await _noteDBContext.Role
                .FirstOrDefaultAsync(r => r.Id == roleId);
        }

        public async Task<IEnumerable<Role>> GetAllRolesAsync()
        {
            return await _noteDBContext.Role.ToListAsync();
        }

        public async Task<Role> AddRoleAsync(Role role)
        {
            var addedRole = await _noteDBContext.Role.AddAsync(role);
            await _noteDBContext.SaveChangesAsync();
            return addedRole.Entity;
        }

        public async Task<IEnumerable<Role>> AddRolesAsync(IEnumerable<Role> roles)
        {
            await _noteDBContext.Role.AddRangeAsync(roles);
            await _noteDBContext.SaveChangesAsync();
            return roles;
        }
    }
}
