namespace Sabanda.Application.Common.Exceptions;

public class NotFoundException : Exception
{
    public NotFoundException(string entityName, object key)
        : base($"{entityName} with key '{key}' was not found.") { }

    public NotFoundException(string message) : base(message) { }
}
