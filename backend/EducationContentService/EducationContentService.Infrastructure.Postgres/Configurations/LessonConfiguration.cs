using EducationContentService.Domain.Lessons;
using EducationContentService.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EducationContentService.Infrastructure.Postgres.Configurations;


public static class Index
{
    public const string TITLE = "ix_lessons_title";
}

public class LessonConfiguration: IEntityTypeConfiguration<Lesson>
{
    public void Configure(EntityTypeBuilder<Lesson> builder)
    {
        builder.ToTable("lessons");

        builder
            .HasKey(l => l.Id)
            .HasName("pk_id");

        builder
            .Property(l => l.Id)
            .HasColumnName("id");

        builder.OwnsOne(l => l.Title, lb =>
        {
            lb.Property(t => t.Value).HasColumnName("title").IsRequired();

            lb
                .HasIndex(t => t.Value).IsUnique()
                .HasDatabaseName(Index.TITLE);
        });


        builder.OwnsOne(l => l.Description, lb =>
        {
            lb
                .Property(d => d.Value)
                .HasColumnName("description")
                .IsRequired();
        });

        builder
            .Property(l => l.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("timezone('utc', now())")
            .HasColumnName("created_at");

        builder
            .Property(l => l.UpdatedAt)
            .HasColumnName("updated_at");

        builder
            .Property(l => l.IsDeleted)
            .HasColumnName("is_deleted");

        builder
            .Property(l => l.DeletedAt)
            .HasColumnName("deleted_at");

        builder.HasQueryFilter(l => !l.IsDeleted);
    }
}