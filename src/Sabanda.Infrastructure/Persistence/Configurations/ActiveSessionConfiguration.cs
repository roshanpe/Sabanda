using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sabanda.Domain.Entities;

namespace Sabanda.Infrastructure.Persistence.Configurations;

public class ActiveSessionConfiguration : IEntityTypeConfiguration<ActiveSession>
{
    public void Configure(EntityTypeBuilder<ActiveSession> builder)
    {
        builder.ToTable("active_sessions");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).HasDefaultValueSql("gen_random_uuid()");
        builder.Property(s => s.CreatedAt).HasDefaultValueSql("now()");
        builder.Property(s => s.Jti).IsRequired().HasMaxLength(100);
        builder.Property(s => s.UserId).IsRequired();

        // This index is hit on every authenticated request
        builder.HasIndex(s => new { s.UserId, s.Jti });
        builder.HasIndex(s => s.Jti).IsUnique();
    }
}
