using Sabanda.Domain.Common;

namespace Sabanda.Domain.Entities;

public class Member : TenantScopedEntity
{
    public Guid FamilyId { get; private set; }
    public string FullName { get; private set; } = string.Empty;
    public DateOnly DateOfBirth { get; private set; }
    // Computed in C# — age() is STABLE not IMMUTABLE so PostgreSQL can't use it in a stored generated column
    public bool IsAdult => DateOfBirth.AddYears(18) <= DateOnly.FromDateTime(DateTime.UtcNow);
    public string? Gender { get; private set; }
    public string? Email { get; private set; }
    public string? Phone { get; private set; }
    public bool IsPrimaryHolder { get; private set; }

    // Minor consent
    public bool ConsentGiven { get; private set; }
    public Guid? ConsentGivenBy { get; private set; }
    public DateTimeOffset? ConsentGivenAt { get; private set; }

    // Optional adult fields
    public string? SkillsJson { get; private set; }
    public string? Occupation { get; private set; }
    public string? BusinessName { get; private set; }

    // QR
    public string? QrToken { get; private set; }
    public Guid? QrTokenJti { get; private set; }
    public DateTimeOffset? QrTokenIssuedAt { get; private set; }

    private Member() { }

    public Member(Guid tenantId, Guid familyId, string fullName, DateOnly dateOfBirth,
        string? gender = null, string? email = null, string? phone = null, bool isPrimaryHolder = false)
    {
        TenantId = tenantId;
        FamilyId = familyId;
        FullName = fullName;
        DateOfBirth = dateOfBirth;
        Gender = gender;
        Email = email;
        Phone = phone;
        IsPrimaryHolder = isPrimaryHolder;
    }

    public void GrantConsent(Guid grantedById, DateTimeOffset at)
    {
        ConsentGiven = true;
        ConsentGivenBy = grantedById;
        ConsentGivenAt = at;
    }

    public void SetAdultFields(string? occupation, string? businessName)
    {
        Occupation = occupation;
        BusinessName = businessName;
    }

    public void SetQrToken(string token, Guid jti)
    {
        QrToken = token;
        QrTokenJti = jti;
        QrTokenIssuedAt = DateTimeOffset.UtcNow;
    }
}
