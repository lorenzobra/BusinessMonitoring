using BusinessMonitoring.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace BusinessMonitoring.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Service> Services => Set<Service>();
    public DbSet<UploadHistory> UploadHistories => Set<UploadHistory>();
    public DbSet<ProcessingError> ProcessingErrors => Set<ProcessingError>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
