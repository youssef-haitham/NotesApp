using NotesApp.API.Common.Dtos;
using NotesApp.API.Common.Exceptions;
using NotesApp.API.Infrastructure.Models;
using NotesApp.API.Interfaces.Repositories;
using NotesApp.API.Interfaces.Services;
using NotesApp.API.Modules.Auth.Dtos.Response;

namespace NotesApp.API.Modules.Auth.Services
{
    public class UserService(IUserRepository userRepo) : IUserService
    {
        private readonly IUserRepository _userRepo = userRepo;
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
    }
}