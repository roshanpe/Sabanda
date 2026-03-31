using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sabanda.Domain.Entities;

namespace Sabanda.Infrastructure.Persistence.Configurations;

public class EventRegistrationConfiguration : IEntityTypeConfiguration<EventRegistration>
{
    public void Configure(EntityTypeBuilder<EventRegistration> builder)
    {
        builder.ToTable("event_registrations");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).HasDefaultValueSql("gen_random_uuid()");
        builder.Property(r => r.CreatedAt).HasDefaultValueSql("now()");
        builder.Property(r => r.TenantId).IsRequired();
        builder.Property(r => r.EventId).IsRequired();
        builder.Property(r => r.FamilyId).IsRequired();
        builder.Property(r => r.Status).HasConversion<string>().IsRequired();
        builder.HasIndex(r => new { r.TenantId, r.EventId, r.Status });
    }
}
