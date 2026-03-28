
namespace ERDM.Infrastructure.Validation
{
    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public List<ValidationError> Errors { get; set; } = new();

        public static ValidationResult Success() => new() { IsValid = true };

        public static ValidationResult Fail(string property, string message)
        {
            return new ValidationResult
            {
                IsValid = false,
                Errors = { new ValidationError { Property = property, Message = message } }
            };
        }

        public static ValidationResult Fail(List<ValidationError> errors)
        {
            return new ValidationResult
            {
                IsValid = false,
                Errors = errors
            };
        }

        public ValidationResult Merge(ValidationResult other)
        {
            if (!other.IsValid)
            {
                IsValid = false;
                Errors.AddRange(other.Errors);
            }
            return this;
        }
    }
}
