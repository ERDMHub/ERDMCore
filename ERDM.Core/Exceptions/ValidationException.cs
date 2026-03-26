namespace ERDM.Core.Exceptions
{
    public class ValidationException : DomainException
    {
        public ValidationException(string property, string message)
            : base($"Validation failed for {property}: {message}")
        {
            Errors = new Dictionary<string, string[]>
            {
                { property, new[] { message } }
            };
        }

        public ValidationException(Dictionary<string, string[]> errors)
            : base("Validation failed")
        {
            Errors = errors;
        }

        public Dictionary<string, string[]> Errors { get; }
    }
}
