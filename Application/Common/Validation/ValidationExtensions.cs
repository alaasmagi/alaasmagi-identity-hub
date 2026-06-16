using FluentValidation.Results;

namespace Application.Common.Validation;

public static class ValidationExtensions
{
    public static string ToErrorMessage(this ValidationResult validationResult)
    {
        return string.Join("; ", validationResult.Errors.Select(error => error.ErrorMessage));
    }
}
