namespace NotesApp.API.Utility
{
    public static class HashHelper
    {
        public static string HashPassword(string password)
        {
            var hashed = BCrypt.Net.BCrypt.HashPassword(password);
            return hashed;
        }

        public static bool Verify(string password, string hashed)
        {
            return BCrypt.Net.BCrypt.Verify(password, hashed);
        }
    }
}
