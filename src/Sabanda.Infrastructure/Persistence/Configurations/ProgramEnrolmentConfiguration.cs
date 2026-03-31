using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sabanda.Domain.Entities;

namespace Sabanda.Infrastructure.Persistence.Configurations;

public class ProgramEnrolmentConfiguration : IEntityTypeConfiguration<ProgramEnrolment>
{
    public void Configure(EntityTypeBuilder<ProgramEnrolment> builder)
    {
        builder.ToTable("program_enrolments");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
        builder.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.ProgramId).IsRequired();
        builder.Property(e => e.MemberId).IsRequired();
        builder.Property(e => e.Status).HasConversion<string>().IsRequired();
        builder.HasIndex(e => new { e.TenantId, e.ProgramId, e.Status });
    }
}
