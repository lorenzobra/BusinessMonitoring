using BusinessMonitoring.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BusinessMonitoring.Infrastructure.Data.Configurations;

public class ProcessingErrorConfiguration : IEntityTypeConfiguration<ProcessingError>
{
    public void Configure(EntityTypeBuilder<ProcessingError> builder)
    {
        builder.ToTable("ProcessingErrors");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasDefaultValueSql("NEWID()");

        builder.Property(e => e.FileName)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(e => e.RowNumber)
            .IsRequired();

        builder.Property(e => e.ErrorMessage)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(e => e.RawData)
            .IsRequired();

        builder.Property(e => e.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("SYSDATETIMEOFFSET()")
            .HasPrecision(0);

        builder.HasIndex(e => e.FileName)
            .HasDatabaseName("IX_ProcessingError_FileName");

        builder.HasIndex(e => e.BatchId)
            .HasDatabaseName("IX_ProcessingError_BatchId");

        builder.HasIndex(e => e.CreatedAt)
            .HasDatabaseName("IX_ProcessingError_CreatedAt");
    }
}