using FluentValidation;
using FluentValidation.Results;

using System.Linq.Expressions;


namespace ERDM.Infrastructure.Validation
{
    public abstract class BaseValidator<T> : AbstractValidator<T>
    {
        protected FluentValidation.Results.ValidationResult ValidateWithResult(T instance)
        {
            return Validate(instance);
        }

        protected async Task<FluentValidation.Results.ValidationResult> ValidateWithResultAsync(T instance, CancellationToken cancellationToken = default)
        {
            return await ValidateAsync(instance, cancellationToken);
        }

        protected void RuleForEmail(Expression<Func<T, string>> expression)
        {
            RuleFor(expression)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Invalid email format")
                .MaximumLength(100).WithMessage("Email cannot exceed 100 characters");
        }

        protected void RuleForPhone(Expression<Func<T, string>> expression)
        {
            RuleFor(expression)
                .NotEmpty().WithMessage("Phone number is required")
                .Matches(@"^\+?[1-9]\d{1,14}$").WithMessage("Invalid phone number format");
        }

        protected void RuleForId(Expression<Func<T, string>> expression)
        {
            RuleFor(expression)
                .NotEmpty().WithMessage("ID is required")
                .MaximumLength(50).WithMessage("ID cannot exceed 50 characters");
        }

        protected void RuleForDate(Expression<Func<T, DateTime?>> expression)
        {
            RuleFor(expression)
                .Must(date => date.HasValue && date.Value <= DateTime.UtcNow)
                .WithMessage("Date cannot be in the future");
        }

        protected void RuleForAmount(Expression<Func<T, decimal>> expression)
        {
            RuleFor(expression)
                .GreaterThan(0).WithMessage("Amount must be greater than zero")
                .LessThanOrEqualTo(1000000).WithMessage("Amount cannot exceed 1,000,000");
        }
    }
}
