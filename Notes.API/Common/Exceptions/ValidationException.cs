namespace NotesApp.API.Common.Exceptions
{
    public class ValidationException : BaseException
    {
        public Dictionary<string, string[]> Errors { get; }

        public ValidationException() 
            : base("One or more validation errors occurred.", StatusCodes.Status400BadRequest, "VALIDATION_ERROR")
        {
            Errors = new Dictionary<string, string[]>();
        }

        public ValidationException(Dictionary<string, string[]> errors) 
            : base("One or more validation errors occurred.", StatusCodes.Status400BadRequest, "VALIDATION_ERROR")
        {
            Errors = errors;
        }

        public ValidationException(string field, string message) 
            : base("One or more validation errors occurred.", StatusCodes.Status400BadRequest, "VALIDATION_ERROR")
        {
            Errors = new Dictionary<string, string[]>
            {
                { field, new[] { message } }
            };
        }
    }
}
