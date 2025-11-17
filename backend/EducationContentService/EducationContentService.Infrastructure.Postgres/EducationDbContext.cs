using EducationContentService.Domain.Lessons;
using EducationContentService.Domain.ModuleItem;
using EducationContentService.Domain.Modules;
using Microsoft.EntityFrameworkCore;

namespace EducationContentService.Infrastructure.Postgres;

public class EducationDbContext: DbContext
{
    public EducationDbContext(DbContextOptions<EducationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Lesson> Lessons => Set<Lesson>();

    // public DbSet<Module> Modules => Set<Module>();

    // public DbSet<ModuleItem> ModuleItems => Set<ModuleItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(EducationDbContext).Assembly);
    }
}