namespace Sabanda.Application.Common.Interfaces;

public interface ICodeGenerator
{
    Task<string> GenerateFamilyCodeAsync(Guid tenantId);
    Task<string> GenerateMemberCodeAsync(Guid tenantId);
}