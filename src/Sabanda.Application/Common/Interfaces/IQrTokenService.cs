namespace Sabanda.Application.Common.Interfaces;

public record QrTokenResult(string Token, Guid Jti);
public record QrLookupResult(string SubjectType, Guid SubjectId, Guid TenantId, Guid Jti);

public interface IQrTokenService
{
    Task<QrTokenResult> IssueAsync(Guid subjectId, string type, Guid tenantId);
    QrLookupResult ValidateAndLookup(string token);
}
