using Application.Services.Errors;

namespace Application.Services.Validation;

public static class Ensure
{
    public static void NotNullOrWhiteSpace(string? v, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(v))
            throw new AppException(new AppError(ErrorCodes.Validation, $"{fieldName} is required."));
    }

    public static void Positive(int v, string fieldName)
    {
        if (v <= 0)
            throw new AppException(new AppError(ErrorCodes.Validation, $"{fieldName} must be > 0."));
    }

    public static void NonNegative(int v, string fieldName)
    {
        if (v < 0)
            throw new AppException(new AppError(ErrorCodes.Validation, $"{fieldName} must be >= 0."));
    }
}
