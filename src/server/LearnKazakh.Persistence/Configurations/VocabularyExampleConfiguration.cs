using LearnKazakh.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LearnKazakh.Persistence.Configurations;

public class VocabularyExampleConfiguration : IEntityTypeConfiguration<VocabularyExample>
{
    public void Configure(EntityTypeBuilder<VocabularyExample> builder)
    {
        // Primary key
        builder.HasKey(ve => ve.Id);

        // Properties
        builder.Property(ve => ve.SentenceKazakh).IsRequired().HasMaxLength(1000);
        builder.Property(ve => ve.SentenceTranslation).IsRequired().HasMaxLength(1000);
        builder.Property(ve => ve.AudioUrl).HasMaxLength(500);
    }
}