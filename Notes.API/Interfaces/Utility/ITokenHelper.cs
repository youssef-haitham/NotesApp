using System.Security.Claims;

namespace NotesApp.API.Interfaces.Utility
{
    public interface ITokenHelper
    {
        string CreateToken(Guid id, string email);
        bool ValidateToken(string token);
        ClaimsPrincipal? GetClaimsFromToken(string token);
    }
}