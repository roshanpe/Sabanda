using FluentValidation;
using Sabanda.Application.Common.Exceptions;
using Sabanda.Application.Common.Interfaces;
using Sabanda.Application.Families.DTOs;
using Sabanda.Domain.Entities;
using Sabanda.Domain.Enums;

namespace Sabanda.Application.Families.Commands;

public class CreateFamilyCommandHandler
{
    private readonly IFamilyRepository _familyRepository;
    private readonly IUserRepository _userRepository;
    private readonly IAuditLogService _auditLog;
    private readonly ICurrentTenantService _tenant;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ICodeGenerator _codeGenerator;
    private readonly IValidator<CreateFamilyRequest> _validator;

    public CreateFamilyCommandHandler(
        IFamilyRepository familyRepository,
        IUserRepository userRepository,
        IAuditLogService auditLog,
        ICurrentTenantService tenant,
        IPasswordHasher passwordHasher,
        ICodeGenerator codeGenerator,
        IValidator<CreateFamilyRequest> validator)
    {
        _familyRepository = familyRepository;
        _userRepository = userRepository;
        _auditLog = auditLog;
        _tenant = tenant;
        _passwordHasher = passwordHasher;
        _codeGenerator = codeGenerator;
        _validator = validator;
    }

    public async Task<FamilyResponse> HandleAsync(CreateFamilyRequest request)
    {
        var result = await _validator.ValidateAsync(request);
        if (!result.IsValid)
            throw new Common.Exceptions.ValidationException(
                result.Errors.GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray()));

        var tenantId = _tenant.TenantId;

        var code = await _codeGenerator.GenerateFamilyCodeAsync(tenantId);

        var existing = await _userRepository.FindByEmailAsync(tenantId, request.PrimaryHolderEmail);
        if (existing != null)
            throw new ConflictException($"Email '{request.PrimaryHolderEmail}' is already registered.");

        // Both entities get client-side GUIDs via BaseEntity initializer
        var family = new Family(tenantId, request.DisplayName, Guid.Empty, code);
        var passwordHash = _passwordHasher.Hash(request.PrimaryHolderPassword);
        var user = new AppUser(tenantId, request.PrimaryHolderEmail, passwordHash,
            UserRole.PrimaryAccountHolder, family.Id);

        // Link family → user
        family.UpdatePrimaryHolder(user.Id);

        await _familyRepository.AddAsync(family);
        await _userRepository.AddAsync(user);
        await _auditLog.LogAsync(AuditAction.FamilyCreated, "Family", family.Id,
            new { primaryHolderEmail = request.PrimaryHolderEmail, primaryHolderUserId = user.Id });
        await _familyRepository.SaveChangesAsync(); // single SaveChanges = single transaction

        return new FamilyResponse(family.Id, family.DisplayName, family.Code, user.Id, false, family.CreatedAt);
    }
}
