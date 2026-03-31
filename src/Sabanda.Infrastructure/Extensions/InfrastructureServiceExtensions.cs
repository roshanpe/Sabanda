using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Sabanda.Application.Common.Interfaces;
using Sabanda.Application.Auth.Commands;
using Sabanda.Application.Families.Commands;
using Sabanda.Application.Families.Queries;
using Sabanda.Application.Members.Commands;
using Sabanda.Application.Members.Queries;
using Sabanda.Application.Qr.Commands;
using Sabanda.Application.Qr.Queries;
using Sabanda.Application.Memberships.Commands;
using Sabanda.Application.Programs.Commands;
using Sabanda.Application.Events.Commands;
using Sabanda.Infrastructure.Persistence;
using Sabanda.Infrastructure.Repositories;
using Sabanda.Infrastructure.Services;

namespace Sabanda.Infrastructure.Extensions;

public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");

        services.AddDbContext<SabandaDbContext>((sp, options) =>
        {
            options.UseNpgsql(connectionString)
                .UseSnakeCaseNamingConvention();
        });

        services.AddHttpContextAccessor();

        // Scoped services — one per HTTP request
        services.AddScoped<ICurrentTenantService, CurrentTenantService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IAuditLogService, AuditLogService>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IQrTokenService, QrTokenService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IFamilyRepository, FamilyRepository>();
        services.AddScoped<IMemberRepository, MemberRepository>();
        services.AddScoped<IMembershipRepository, MembershipRepository>();
        services.AddScoped<IProgramRepository, ProgramRepository>();
        services.AddScoped<IProgramEnrolmentRepository, ProgramEnrolmentRepository>();
        services.AddScoped<IEventRepository, EventRepository>();
        services.AddScoped<IEventRegistrationRepository, EventRegistrationRepository>();
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
        services.AddSingleton<IPasswordHasher, PasswordHasher>();

        // FluentValidation — scan Application assembly
        services.AddValidatorsFromAssembly(typeof(Sabanda.Application.AssemblyMarker).Assembly);

        // Application command/query handlers
        services.AddScoped<LoginCommandHandler>();
        services.AddScoped<LogoutCommandHandler>();
        services.AddScoped<CreateFamilyCommandHandler>();
        services.AddScoped<GetFamilyQueryHandler>();
        services.AddScoped<GetFamilySummaryQueryHandler>();
        services.AddScoped<CreateMemberCommandHandler>();
        services.AddScoped<GetMemberQueryHandler>();
        services.AddScoped<RegenerateFamilyQrCommandHandler>();
        services.AddScoped<RegenerateMemberQrCommandHandler>();
        services.AddScoped<QrLookupQueryHandler>();
        services.AddScoped<CreateMembershipCommandHandler>();
        services.AddScoped<UpdatePaymentStatusCommandHandler>();
        services.AddScoped<CreateProgramCommandHandler>();
        services.AddScoped<EnrolMemberCommandHandler>();
        services.AddScoped<CancelEnrolmentCommandHandler>();
        services.AddScoped<CreateEventCommandHandler>();
        services.AddScoped<RegisterEventCommandHandler>();
        services.AddScoped<CancelEventRegistrationCommandHandler>();

        return services;
    }
}
