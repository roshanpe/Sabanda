using FluentValidation;
using Sabanda.Application.Common.Exceptions;
using Sabanda.Application.Common.Interfaces;
using Sabanda.Application.Programs.DTOs;
using Sabanda.Domain.Entities;
using Sabanda.Domain.Enums;
using Program = Sabanda.Domain.Entities.Program;

namespace Sabanda.Application.Programs.Commands;

public class CreateProgramCommandHandler
{
    private readonly IProgramRepository _programRepo;
    private readonly ICurrentTenantService _tenant;
    private readonly IValidator<CreateProgramRequest> _validator;

    public CreateProgramCommandHandler(
        IProgramRepository programRepo,
        ICurrentTenantService tenant,
        IValidator<CreateProgramRequest> validator)
    {
        _programRepo = programRepo;
        _tenant = tenant;
        _validator = validator;
    }

    public async Task<ProgramResponse> HandleAsync(CreateProgramRequest request)
    {
        var result = await _validator.ValidateAsync(request);
        if (!result.IsValid)
            throw new Common.Exceptions.ValidationException(
                result.Errors.GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray()));

        var program = new Program(
            _tenant.TenantId,
            request.Name,
            request.Capacity,
            request.Description,
            request.CoordinatorUserId,
            request.AgeGroup,
            request.Frequency,
            request.Venue,
            request.Day,
            request.Time);

        await _programRepo.AddAsync(program);
        await _programRepo.SaveChangesAsync();

        return ToResponse(program);
    }

    internal static ProgramResponse ToResponse(Program p) =>
        new(
            p.Id,
            p.Name,
            p.Description,
            p.Capacity,
            p.CoordinatorUserId,
            p.AgeGroup,
            p.Frequency,
            p.Venue,
            p.Day,
            p.Time,
            p.CreatedAt);
}

public class EnrolMemberCommandHandler
{
    private readonly IProgramRepository _programRepo;
    private readonly IProgramEnrolmentRepository _enrolmentRepo;
    private readonly IMembershipRepository _membershipRepo;
    private readonly IMemberRepository _memberRepo;
    private readonly ICurrentTenantService _tenant;

    public EnrolMemberCommandHandler(
        IProgramRepository programRepo,
        IProgramEnrolmentRepository enrolmentRepo,
        IMembershipRepository membershipRepo,
        IMemberRepository memberRepo,
        ICurrentTenantService tenant)
    {
        _programRepo = programRepo;
        _enrolmentRepo = enrolmentRepo;
        _membershipRepo = membershipRepo;
        _memberRepo = memberRepo;
        _tenant = tenant;
    }

    public async Task<EnrolmentResponse> HandleAsync(Guid programId, EnrolMemberRequest request)
    {
        var program = await _programRepo.FindByIdAsync(programId)
            ?? throw new NotFoundException($"Program {programId} not found.");

        var member = await _memberRepo.FindByIdAsync(request.MemberId)
            ?? throw new NotFoundException($"Member {request.MemberId} not found.");

        // Verify active Program membership for the member's family
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var membership = await _membershipRepo.FindActiveAsync(member.FamilyId, MembershipType.Program, today);
        if (membership == null)
            throw new Common.Exceptions.ValidationException(
                new Dictionary<string, string[]>
                {
                    ["Membership"] = ["An active Program membership is required to enrol."]
                });

        // Prevent duplicate enrolments
        var alreadyEnrolled = await _enrolmentRepo.IsMemberEnrolledOrWaitlistedAsync(programId, request.MemberId);
        if (alreadyEnrolled)
            throw new ConflictException("Member is already enrolled or waitlisted in this program.");

        var enrolledCount = await _enrolmentRepo.CountEnrolledAsync(programId);
        EnrolmentStatus status;
        int? waitlistPosition = null;

        if (enrolledCount < program.Capacity)
        {
            status = EnrolmentStatus.Enrolled;
        }
        else
        {
            status = EnrolmentStatus.Waitlisted;
            waitlistPosition = await _enrolmentRepo.GetMaxWaitlistPositionAsync(programId) + 1;
        }

        var enrolment = new ProgramEnrolment(_tenant.TenantId, programId, request.MemberId,
            status, waitlistPosition);

        await _enrolmentRepo.AddAsync(enrolment);
        await _enrolmentRepo.SaveChangesAsync();

        return ToResponse(enrolment);
    }

    internal static EnrolmentResponse ToResponse(ProgramEnrolment e) =>
        new(e.Id, e.ProgramId, e.MemberId, e.Status, e.WaitlistPosition, e.EnrolledAt, e.CancelledAt);
}

public class CancelEnrolmentCommandHandler
{
    private readonly IProgramEnrolmentRepository _enrolmentRepo;

    public CancelEnrolmentCommandHandler(IProgramEnrolmentRepository enrolmentRepo)
    {
        _enrolmentRepo = enrolmentRepo;
    }

    public async Task HandleAsync(Guid programId, Guid enrolmentId)
    {
        var enrolment = await _enrolmentRepo.FindByIdAsync(enrolmentId)
            ?? throw new NotFoundException($"Enrolment {enrolmentId} not found.");

        if (enrolment.ProgramId != programId)
            throw new NotFoundException($"Enrolment {enrolmentId} not found in program {programId}.");

        if (enrolment.Status == EnrolmentStatus.Cancelled)
            throw new ConflictException("Enrolment is already cancelled.");

        var wasEnrolled = enrolment.Status == EnrolmentStatus.Enrolled;
        var oldPosition = enrolment.WaitlistPosition;

        enrolment.Cancel();

        if (wasEnrolled)
        {
            // Promote the first waitlisted member — atomic with the cancellation
            var first = await _enrolmentRepo.GetFirstWaitlistedAsync(programId);
            if (first != null)
            {
                var promotedPos = first.WaitlistPosition!.Value;
                first.Promote();
                var remaining = await _enrolmentRepo.GetWaitlistedAfterPositionAsync(programId, promotedPos);
                foreach (var r in remaining)
                    r.DecrementWaitlistPosition();
            }
        }
        else if (oldPosition.HasValue)
        {
            // Compact the waitlist above this position
            var remaining = await _enrolmentRepo.GetWaitlistedAfterPositionAsync(programId, oldPosition.Value);
            foreach (var r in remaining)
                r.DecrementWaitlistPosition();
        }

        await _enrolmentRepo.SaveChangesAsync(); // single transaction covers all changes
    }
}
