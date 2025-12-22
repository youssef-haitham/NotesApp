using NotesApp.API.Common.Dtos;
using NotesApp.API.Common.Exceptions;
using NotesApp.API.Infrastructure.Models;
using NotesApp.API.Interfaces.Repositories;
using NotesApp.API.Interfaces.Services;
using NotesApp.API.Interfaces.Utility;
using NotesApp.API.Modules.Auth.Dtos.Request;
using NotesApp.API.Modules.Auth.Dtos.Response;

namespace NotesApp.API.Modules.Auth.Services
{
    public class UserService(IUserRepository userRepo, IHashProvider hashProvider) : IUserService
    {
        private readonly IUserRepository _userRepo = userRepo;
        private readonly IHashProvider _hashProvider = hashProvider;
        public async Task<UserDto> GetUserById(Guid id)
        {
            User? user = await _userRepo.GetUserByIdAsync(id);

            if (user == null)
            {
                throw new NotFoundException("User", id);
            }

            return new UserDto { Id = user.Id, Email = user.Email, Name = user.Name };
        }

        public async Task<PagedResponseDto<UserDto>> GetUsersAsync(int pageNumber, int pageSize)
        {
            if (pageNumber < 1)
            {
                throw new BadRequestException("Page number must be greater than 0");
            }

            if (pageSize < 1 || pageSize > 100)
            {
                throw new BadRequestException("Page size must be between 1 and 100");
            }

            var (users, totalCount) = await _userRepo.GetUsersAsync(pageNumber, pageSize);

            var userDtos = users.Select(u => new UserDto
            {
                Id = u.Id,
                Email = u.Email,
                Name = u.Name
            });

            return new PagedResponseDto<UserDto>
            {
                Data = userDtos,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }

        public async Task UpdatePasswordAsync(Guid userId, UpdatePasswordRequestDto request)
        {
            var user = await _userRepo.GetUserByIdAsync(userId);

            if (user == null)
            {
                throw new NotFoundException("User", userId);
            }

            if (!_hashProvider.Verify(request.CurrentPassword, user.PasswordHash))
            {
                throw new BadRequestException("Current password is incorrect");
            }

            if (request.CurrentPassword == request.NewPassword)
            {
                throw new BadRequestException("New password must be different from current password");
            }

            user.PasswordHash = _hashProvider.HashPassword(request.NewPassword);
            user.UpdatedAt = DateTime.UtcNow;

            await _userRepo.UpdateUserAsync(user);
        }
    }
}