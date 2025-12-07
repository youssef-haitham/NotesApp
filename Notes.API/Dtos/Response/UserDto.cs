namespace NotesApp.API.Dtos.Response
{
    public class UserDto
    {
        public required Guid Id { get; set; }
        public required string Name { get; set; }
        public required string Email { get; set; }
    }
}
