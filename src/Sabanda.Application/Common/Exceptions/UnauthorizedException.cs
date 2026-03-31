namespace Sabanda.Application.Common.Exceptions;

public class UnauthorizedException : Exception
{
    public UnauthorizedException(string message = "Authentication is required.") : base(message) { }
}
