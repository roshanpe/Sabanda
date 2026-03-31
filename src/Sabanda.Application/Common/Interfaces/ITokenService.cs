using Sabanda.Domain.Entities;

namespace Sabanda.Application.Common.Interfaces;

public interface ITokenService
{
    Task<string> IssueTokenAsync(AppUser user);
    Task RevokeAsync(string jti);
    Task RevokeAllForUserAsync(Guid userId);
}
