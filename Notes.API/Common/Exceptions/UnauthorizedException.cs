namespace NotesApp.API.Common.Exceptions
{
    public class UnauthorizedException : BaseException
    {
        public UnauthorizedException(string message = "Unauthorized access") 
            : base(message, StatusCodes.Status401Unauthorized, "UNAUTHORIZED")
        {
        }
    }
}
