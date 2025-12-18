namespace NotesApp.API.Interfaces.Utility
{
    public interface IHashProvider
    {
        string HashPassword(string password);
        bool Verify(string password, string hashed);
    }
}