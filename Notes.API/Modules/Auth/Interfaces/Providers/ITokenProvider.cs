using System.Security.Claims;

namespace NotesApp.API.Interfaces.Utility
{
    public interface ITokenProvider
    {
        string CreateToken(Guid id, string email, string role);
        bool ValidateToken(string token);
        ClaimsPrincipal? GetClaimsFromToken(string token);
    }
}