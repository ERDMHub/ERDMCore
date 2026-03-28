using FluentValidation;

namespace ERDM.Infrastructure.Validation
{
    public static class ValidationExtensions
    {
        public static async Task<ValidationResult> ValidateAsync<T>(
            this IValidator<T> validator,
            T instance,
            CancellationToken cancellationToken = default)
        {
            var result = await validator.ValidateAsync(instance, cancellationToken);
            return result.ToValidationResult();
        }

        public static ValidationResult ToValidationResult(this FluentValidation.Results.ValidationResult result)
        {
            if (result.IsValid)
                return ValidationResult.Success();

            return ValidationResult.Fail(result.Errors.Select(e => new ValidationError
            {
                Property = e.PropertyName,
                Message = e.ErrorMessage,
                AttemptedValue = e.AttemptedValue
            }).ToList());
        }

        public static void AddToModelState(this ValidationResult result, Microsoft.AspNetCore.Mvc.ModelBinding.ModelStateDictionary modelState)
        {
            foreach (var error in result.Errors)
            {
                modelState.AddModelError(error.Property, error.Message);
            }
        }

        public static bool IsValidResponse(this ValidationResult result, out List<string> errors)
        {
            errors = result.Errors.Select(e => e.Message).ToList();
            return result.IsValid;
        }
    }
}
