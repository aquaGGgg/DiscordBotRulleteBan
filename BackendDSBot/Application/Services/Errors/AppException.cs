namespace Application.Services.Errors;

public sealed class AppException : Exception
{
    public AppError Error { get; }

    public AppException(AppError error) : base(error.Message)
    {
        Error = error;
    }
}
