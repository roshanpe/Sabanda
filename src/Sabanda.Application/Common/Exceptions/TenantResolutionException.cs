namespace Sabanda.Application.Common.Exceptions;

public class TenantResolutionException : Exception
{
    public TenantResolutionException(string message = "Tenant could not be resolved from the request.")
        : base(message) { }
}
