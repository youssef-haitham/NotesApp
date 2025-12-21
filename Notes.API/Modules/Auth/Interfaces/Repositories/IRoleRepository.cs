using NotesApp.API.Infrastructure.Models;

namespace NotesApp.API.Modules.Auth.Interfaces.Repositories
{
    public interface IRoleRepository
    {
        Task<Role?> GetRoleByNameAsync(string roleName);
        Task<Role?> GetRoleByIdAsync(int roleId);
        Task<IEnumerable<Role>> GetAllRolesAsync();
        Task<Role> AddRoleAsync(Role role);
        Task<IEnumerable<Role>> AddRolesAsync(IEnumerable<Role> roles);
    }
}

