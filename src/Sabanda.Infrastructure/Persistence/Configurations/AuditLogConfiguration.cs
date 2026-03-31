using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sabanda.Domain.Entities;

namespace Sabanda.Infrastructure.Persistence.Configurations;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("audit_logs");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).HasDefaultValueSql("gen_random_uuid()");
        builder.Property(a => a.CreatedAt).HasDefaultValueSql("now()");
        builder.Property(a => a.TenantId).IsRequired();
        builder.Property(a => a.Action).HasConversion<string>().IsRequired();
        builder.Property(a => a.TargetEntityType).HasMaxLength(100);
        builder.Property(a => a.TargetEntityId);
        builder.Property(a => a.DetailJson).HasColumnType("jsonb");

        // No navigation properties - AuditLog is write-only from application perspective
        builder.HasIndex(a => new { a.TenantId, a.CreatedAt });
        builder.HasIndex(a => new { a.TenantId, a.UserId });
    }
}
