using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sabanda.Domain.Entities;

namespace Sabanda.Infrastructure.Persistence.Configurations;

public class MembershipConfiguration : IEntityTypeConfiguration<Membership>
{
    public void Configure(EntityTypeBuilder<Membership> builder)
    {
        builder.ToTable("memberships");
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).HasDefaultValueSql("gen_random_uuid()");
        builder.Property(m => m.CreatedAt).HasDefaultValueSql("now()");
        builder.Property(m => m.TenantId).IsRequired();
        builder.Property(m => m.FamilyId).IsRequired();
        builder.Property(m => m.Type).HasConversion<string>().IsRequired();
        builder.Property(m => m.PaymentStatus).HasConversion<string>().IsRequired();
        builder.HasIndex(m => new { m.TenantId, m.FamilyId, m.Type });
    }
}
