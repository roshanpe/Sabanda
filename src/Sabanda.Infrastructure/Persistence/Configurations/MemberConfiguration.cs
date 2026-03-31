using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sabanda.Domain.Entities;

namespace Sabanda.Infrastructure.Persistence.Configurations;

public class MemberConfiguration : IEntityTypeConfiguration<Member>
{
    public void Configure(EntityTypeBuilder<Member> builder)
    {
        builder.ToTable("members");
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).HasDefaultValueSql("gen_random_uuid()");
        builder.Property(m => m.CreatedAt).HasDefaultValueSql("now()");
        builder.Property(m => m.TenantId).IsRequired();
        builder.Property(m => m.FamilyId).IsRequired();
        builder.Property(m => m.FullName).IsRequired().HasMaxLength(200);
        builder.Property(m => m.Email).HasMaxLength(200);
        builder.Property(m => m.Phone).HasMaxLength(50);
        builder.Property(m => m.Gender).HasMaxLength(50);
        builder.Property(m => m.SkillsJson).HasColumnType("jsonb");

        // IsAdult is computed in C# (age() is STABLE not IMMUTABLE in PostgreSQL,
        // so it cannot be used in a stored generated column)
        builder.Ignore(m => m.IsAdult);

        builder.HasIndex(m => new { m.TenantId, m.QrTokenJti });
        builder.HasIndex(m => new { m.TenantId, m.FamilyId });
    }
}
