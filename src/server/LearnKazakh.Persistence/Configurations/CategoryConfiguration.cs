using LearnKazakh.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LearnKazakh.Persistence.Configurations;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        // Primary key
        builder.HasKey(c => c.Id);

        // Properties
        builder.Property(c => c.Name).IsRequired().HasMaxLength(100);
        builder.Property(c => c.Description).IsRequired().HasMaxLength(400);
        builder.Property(c => c.Icon).IsRequired().HasMaxLength(1000);

        // Relationships
        builder
            .HasMany(c => c.Sections)
            .WithOne(s => s.Category)
            .HasForeignKey(s => s.CategoryId);

        // Indexes
        builder
            .HasIndex(c => c.Name)
            .HasDatabaseName("IX_Categories_Name");
    }
}