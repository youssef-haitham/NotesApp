using NotesApp.API.Interfaces.Utility;

namespace NotesApp.API.Modules.Auth.Utility
{
    public class HashProvider : IHashProvider
    {
        public string HashPassword(string password)
        {
            var hashed = BCrypt.Net.BCrypt.HashPassword(password);
            return hashed;
        }

        public bool Verify(string password, string hashed)
        {
            return BCrypt.Net.BCrypt.Verify(password, hashed);
        }
    }
}