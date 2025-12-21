namespace NotesApp.API.Common.Exceptions
{
    public class ConflictException : BaseException
    {
        public ConflictException(string message) 
            : base(message, StatusCodes.Status409Conflict, "CONFLICT")
        {
        }

        public ConflictException(string resourceName, string field, object value) 
            : base($"{resourceName} with {field} '{value}' already exists.", StatusCodes.Status409Conflict, "CONFLICT")
        {
        }
    }
}
