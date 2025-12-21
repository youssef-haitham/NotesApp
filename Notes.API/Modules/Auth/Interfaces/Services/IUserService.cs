using NotesApp.API.Common.Dtos;
using NotesApp.API.Modules.Auth.Dtos.Response;

namespace NotesApp.API.Interfaces.Services
{
    public interface IUserService
    {
        Task<UserDto> GetUserById(Guid id);
        Task<PagedResponseDto<UserDto>> GetUsersAsync(int pageNumber, int pageSize);
    }
}
