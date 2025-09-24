using LearnKazakh.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LearnKazakh.Persistence.Configurations;

public class ContentConfiguration : IEntityTypeConfiguration<Content>
{
    public void Configure(EntityTypeBuilder<Content> builder)
    {
        // Primary key
        builder.HasKey(c => c.Id);

        // Properties
        builder.Property(c => c.ContentText).IsRequired().HasMaxLength(10000);
        builder.Property(c => c.ContentMarkdown).HasMaxLength(10000);
        builder.Property(c => c.ContentHtml).HasMaxLength(10000);
    }
}