using BusinessMonitoring.Core.Entities;
using BusinessMonitoring.Core.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BusinessMonitoring.Infrastructure.Data.Configurations;

public class UploadHistoryConfiguration : IEntityTypeConfiguration<UploadHistory>
{
    public void Configure(EntityTypeBuilder<UploadHistory> builder)
    {
        builder.ToTable("UploadHistories");

        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id).HasDefaultValueSql("NEWID()");

        builder.Property(u => u.FileName)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(u => u.UploadedBy)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(u => u.UploadedAt)
            .IsRequired()
            .HasDefaultValueSql("SYSDATETIMEOFFSET()")
            .HasPrecision(0);

        builder.Property(u => u.ProcessingStartedAt)
          .HasPrecision(0);

        builder.Property(u => u.ProcessingCompletedAt)
        .HasPrecision(0);


        builder.Property(u => u.ProcessingStatus)
            .HasConversion<string>()
            .IsRequired()
            .HasDefaultValue(ProcessingStatus.Pending);

        builder.Property(u => u.ErrorMessage)
            .HasMaxLength(2000);

        builder.HasIndex(u => u.FileName)
            .HasDatabaseName("IX_UploadHistory_FileName");

        builder.HasIndex(u => u.UploadedAt)
            .HasDatabaseName("IX_UploadHistory_UploadedAt");

        builder.HasIndex(u => u.BatchId)
            .HasDatabaseName("IX_UploadHistory_BatchId");
    }
}
