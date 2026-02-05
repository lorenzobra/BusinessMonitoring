using BusinessMonitoring.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BusinessMonitoring.Infrastructure.Data.Configurations;
public class ServiceConfiguration : IEntityTypeConfiguration<Service>
{
    public void Configure(EntityTypeBuilder<Service> builder)
    {
        builder.ToTable("Services");

        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).HasDefaultValueSql("NEWID()");

        builder.HasIndex(s => new { s.CustomerId, s.ServiceType, s.ActivationDate })
            .IsUnique();

        builder.Property(s => s.CustomerId)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(s => s.ServiceType)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(s => s.ActivationDate)
            .IsRequired()
             .HasPrecision(0);

        builder.Property(s => s.ExpirationDate)
            .IsRequired()
             .HasPrecision(0);

        builder.Property(s => s.Amount)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.Property(s => s.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(s => s.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("SYSDATETIMEOFFSET()")
            .HasPrecision(0);

        builder.Property(s => s.UpdatedAt)
            .HasPrecision(0); ;

        builder.Property(s => s.ImportedFromFile)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(s => s.UpdatedFromFile)
            .HasMaxLength(255);

        builder.HasIndex(s => new { s.CustomerId, s.Status });

        builder.HasIndex(s => new { s.ServiceType, s.Status });

        builder.HasIndex(s => s.ExpirationDate);

        builder.HasIndex(s => s.ActivationDate);
    }
}