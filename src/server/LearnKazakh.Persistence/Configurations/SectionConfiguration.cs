using LearnKazakh.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LearnKazakh.Persistence.Configurations;

public class SectionConfiguration : IEntityTypeConfiguration<Section>
{
    public void Configure(EntityTypeBuilder<Section> builder)
    {
        // Primary key
        builder.HasKey(s => s.Id);

        // Properties
        builder.Property(s => s.Title).IsRequired().HasMaxLength(50);

        // Relationships
        builder
            .HasMany(s => s.Contents)
            .WithOne(c => c.Section)
            .HasForeignKey(c => c.SectionId);

        // Indexes
        builder
            .HasIndex(s => s.Title)
            .HasDatabaseName("IX_Sections_Title");
    }
}