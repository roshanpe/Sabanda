using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sabanda.Domain.Entities;

namespace Sabanda.Infrastructure.Persistence.Configurations;

public class FamilyConfiguration : IEntityTypeConfiguration<Family>
{
    public void Configure(EntityTypeBuilder<Family> builder)
    {
        builder.ToTable("families");
        builder.HasKey(f => f.Id);
        builder.Property(f => f.Id).HasDefaultValueSql("gen_random_uuid()");
        builder.Property(f => f.CreatedAt).HasDefaultValueSql("now()");
        builder.Property(f => f.TenantId).IsRequired();
        builder.Property(f => f.DisplayName).IsRequired().HasMaxLength(200);
        builder.HasIndex(f => new { f.TenantId, f.QrTokenJti });
    }
}
