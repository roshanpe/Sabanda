using FluentValidation;
using Sabanda.Application.Common.Exceptions;
using Sabanda.Application.Common.Interfaces;
using Sabanda.Application.Events.DTOs;
using Sabanda.Domain.Entities;
using Sabanda.Domain.Enums;

namespace Sabanda.Application.Events.Commands;

public class CreateEventCommandHandler
{
    private readonly IEventRepository _eventRepo;
    private readonly ICurrentTenantService _tenant;
    private readonly IValidator<CreateEventRequest> _validator;

    public CreateEventCommandHandler(
        IEventRepository eventRepo,
        ICurrentTenantService tenant,
        IValidator<CreateEventRequest> validator)
    {
        _eventRepo = eventRepo;
        _tenant = tenant;
        _validator = validator;
    }

    public async Task<EventResponse> HandleAsync(CreateEventRequest request)
    {
        var result = await _validator.ValidateAsync(request);
        if (!result.IsValid)
            throw new Common.Exceptions.ValidationException(
                result.Errors.GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray()));

        var @event = new Event(_tenant.TenantId, request.Name, request.EventDate, request.Capacity,
            request.BillingType, request.Description, request.CoordinatorUserId);

        await _eventRepo.AddAsync(@event);
        await _eventRepo.SaveChangesAsync();

        return ToResponse(@event);
    }

    internal static EventResponse ToResponse(Event e) =>
        new(e.Id, e.Name, e.Description, e.EventDate, e.Capacity, e.BillingType,
            e.CoordinatorUserId, e.CreatedAt);
}

public class RegisterEventCommandHandler
{
    private readonly IEventRepository _eventRepo;
    private readonly IEventRegistrationRepository _registrationRepo;
    private readonly IMembershipRepository _membershipRepo;
    private readonly ICurrentUserService _currentUser;
    private readonly ICurrentTenantService _tenant;

    public RegisterEventCommandHandler(
        IEventRepository eventRepo,
        IEventRegistrationRepository registrationRepo,
        IMembershipRepository membershipRepo,
        ICurrentUserService currentUser,
        ICurrentTenantService tenant)
    {
        _eventRepo = eventRepo;
        _registrationRepo = registrationRepo;
        _membershipRepo = membershipRepo;
        _currentUser = currentUser;
        _tenant = tenant;
    }

    public async Task<RegistrationResponse> HandleAsync(Guid eventId, RegisterEventRequest request)
    {
        // FamilyMember role cannot register for events
        if (_currentUser.Role == UserRole.FamilyMember)
            throw new ForbiddenException("FamilyMember role cannot register for events.");

        var @event = await _eventRepo.FindByIdAsync(eventId)
            ?? throw new NotFoundException($"Event {eventId} not found.");

        // Verify active Event membership for the family
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var membership = await _membershipRepo.FindActiveAsync(request.FamilyId, MembershipType.Event, today);
        if (membership == null)
            throw new Common.Exceptions.ValidationException(
                new Dictionary<string, string[]>
                {
                    ["Membership"] = ["An active Event membership is required to register."]
                });

        // For Individual billing, MemberId is required
        if (@event.BillingType == EventBillingType.Individual && request.MemberId == null)
            throw new Common.Exceptions.ValidationException(
                new Dictionary<string, string[]>
                {
                    ["MemberId"] = ["MemberId is required for Individual billing events."]
                });

        // Prevent duplicate registrations
        var isDuplicate = await _registrationRepo.IsDuplicateAsync(eventId, request.FamilyId, request.MemberId);
        if (isDuplicate)
            throw new ConflictException("This family/member is already registered or waitlisted for this event.");

        var registeredCount = await _registrationRepo.CountRegisteredAsync(eventId);
        RegistrationStatus status;
        int? waitlistPosition = null;

        if (registeredCount < @event.Capacity)
        {
            status = RegistrationStatus.Registered;
        }
        else
        {
            status = RegistrationStatus.Waitlisted;
            waitlistPosition = await _registrationRepo.GetMaxWaitlistPositionAsync(eventId) + 1;
        }

        var registration = new EventRegistration(_tenant.TenantId, eventId, request.FamilyId,
            status, request.MemberId, waitlistPosition);

        await _registrationRepo.AddAsync(registration);
        await _registrationRepo.SaveChangesAsync();

        return ToResponse(registration);
    }

    internal static RegistrationResponse ToResponse(EventRegistration r) =>
        new(r.Id, r.EventId, r.FamilyId, r.MemberId, r.Status, r.WaitlistPosition,
            r.RegisteredAt, r.CancelledAt);
}

public class CancelEventRegistrationCommandHandler
{
    private readonly IEventRegistrationRepository _registrationRepo;

    public CancelEventRegistrationCommandHandler(IEventRegistrationRepository registrationRepo)
    {
        _registrationRepo = registrationRepo;
    }

    public async Task HandleAsync(Guid eventId, Guid registrationId)
    {
        var registration = await _registrationRepo.FindByIdAsync(registrationId)
            ?? throw new NotFoundException($"Registration {registrationId} not found.");

        if (registration.EventId != eventId)
            throw new NotFoundException($"Registration {registrationId} not found in event {eventId}.");

        if (registration.Status == RegistrationStatus.Cancelled)
            throw new ConflictException("Registration is already cancelled.");

        var wasRegistered = registration.Status == RegistrationStatus.Registered;
        var oldPosition = registration.WaitlistPosition;

        registration.Cancel();

        if (wasRegistered)
        {
            var first = await _registrationRepo.GetFirstWaitlistedAsync(eventId);
            if (first != null)
            {
                var promotedPos = first.WaitlistPosition!.Value;
                first.Promote();
                var remaining = await _registrationRepo.GetWaitlistedAfterPositionAsync(eventId, promotedPos);
                foreach (var r in remaining)
                    r.DecrementWaitlistPosition();
            }
        }
        else if (oldPosition.HasValue)
        {
            var remaining = await _registrationRepo.GetWaitlistedAfterPositionAsync(eventId, oldPosition.Value);
            foreach (var r in remaining)
                r.DecrementWaitlistPosition();
        }

        await _registrationRepo.SaveChangesAsync();
    }
}
