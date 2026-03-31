namespace Sabanda.Application.Common.Exceptions;

public class ForbiddenException : Exception
{
    public ForbiddenException(string message = "Access denied.") : base(message) { }
}
