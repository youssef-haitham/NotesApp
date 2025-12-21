using NotesApp.API.Infrastructure.Models;
using NotesApp.API.Interfaces.Repositories;
using NotesApp.API.Interfaces.Utility;
using NotesApp.API.Modules.Auth.Interfaces.Repositories;

namespace NotesApp.API.Infrastructure.Data
{
    public static class DataSeeder
    {
        public static async Task SeedRolesAsync(IRoleRepository roleRepository)
        {
            var existingRoles = await roleRepository.GetAllRolesAsync();
            if (existingRoles.Any())
            {
                return;
            }

            var roles = new[]
            {
                new Role
                {
                    Name = "User",
                    Description = "Regular user role"
                },
                new Role
                {
                    Name = "Admin",
                    Description = "Administrator role"
                }
            };

            await roleRepository.AddRolesAsync(roles);
        }

        public static async Task SeedAdminUserAsync(
            IUserRepository userRepository,
            IRoleRepository roleRepository,
            IHashProvider hashProvider,
            IConfiguration configuration)
        {
            var adminEmail = Environment.GetEnvironmentVariable("ADMIN_EMAIL")
                ?? configuration["Admin:Email"]
                ?? "admin@notesapp.com";
            
            var adminPassword = Environment.GetEnvironmentVariable("ADMIN_PASSWORD")
                ?? configuration["Admin:Password"]
                ?? "Admin@123";
            
            var adminName = Environment.GetEnvironmentVariable("ADMIN_NAME")
                ?? configuration["Admin:Name"]
                ?? "Admin User";

            var adminExists = await userRepository.UserExistsByEmailAsync(adminEmail);
            if (adminExists)
            {
                return;
            }

            var adminRole = await roleRepository.GetRoleByNameAsync("Admin");
            if (adminRole == null)
            {
                throw new Exception("Admin role not found. Please seed roles first.");
            }

            var adminUser = new User
            {
                Name = adminName,
                Email = adminEmail,
                PasswordHash = hashProvider.HashPassword(adminPassword),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var addedAdmin = await userRepository.AddUserAsync(adminUser);

            var userRole = new UserRole
            {
                UserId = addedAdmin.Id,
                RoleId = adminRole.Id,
                User = addedAdmin,
                Role = adminRole
            };

            await userRepository.AddUserRoleAsync(userRole);
        }
    }
}