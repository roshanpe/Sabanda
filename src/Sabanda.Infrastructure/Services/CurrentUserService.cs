using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Sabanda.Application.Common.Interfaces;
using Sabanda.Domain.Enums;

namespace Sabanda.Infrastructure.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid UserId
    {
        get
        {
            var value = _httpContextAccessor.HttpContext?.User.FindFirstValue(JwtRegisteredClaimNames.Sub);
            return value != null ? Guid.Parse(value) : Guid.Empty;
        }
    }

    public UserRole Role
    {
        get
        {
            var value = _httpContextAccessor.HttpContext?.User.FindFirstValue("role");
            return value != null && Enum.TryParse<UserRole>(value, out var role) ? role : UserRole.FamilyMember;
        }
    }

    public Guid? FamilyId
    {
        get
        {
            var value = _httpContextAccessor.HttpContext?.User.FindFirstValue("family_id");
            return value != null && Guid.TryParse(value, out var id) ? id : null;
        }
    }

    public bool IsAuthenticated =>
        _httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated == true;
}
