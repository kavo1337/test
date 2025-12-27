namespace MVVM.WEB.Services;

public sealed class ApiException : Exception
{
    public ApiException(string message)
        : base(message)
    {
    }
}
