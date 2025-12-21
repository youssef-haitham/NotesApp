namespace NotesApp.API.Common.Exceptions
{
    public class NotFoundException : BaseException
    {
        public NotFoundException(string message) 
            : base(message, StatusCodes.Status404NotFound, "NOT_FOUND")
        {
        }

        public NotFoundException(string resourceName, object key) 
            : base($"{resourceName} with id '{key}' was not found.", StatusCodes.Status404NotFound, "NOT_FOUND")
        {
        }
    }
}
