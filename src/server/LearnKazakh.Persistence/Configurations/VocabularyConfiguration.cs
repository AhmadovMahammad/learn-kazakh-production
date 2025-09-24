using LearnKazakh.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LearnKazakh.Persistence.Configurations;

public class VocabularyConfiguration : IEntityTypeConfiguration<Vocabulary>
{
    public void Configure(EntityTypeBuilder<Vocabulary> builder)
    {
        // Primary key
        builder.HasKey(v => v.Id);

        // Properties
        builder.Property(v => v.WordKazakh).IsRequired().HasMaxLength(200);
        builder.Property(v => v.WordAzerbaijani).IsRequired().HasMaxLength(200);
        builder.Property(v => v.Pronounciation).HasMaxLength(500);
        builder.Property(v => v.AudioUrl).HasMaxLength(500);

        // Relationships
        builder
            .HasMany(v => v.VocabularyExamples)
            .WithOne(ve => ve.Vocabulary)
            .HasForeignKey(ve => ve.VocabularyId);

        // Indexes
        builder
            .HasIndex(v => v.WordKazakh)
            .HasDatabaseName("IX_Vocabulary_WordKazakh");

        builder
            .HasIndex(v => v.WordAzerbaijani)
            .HasDatabaseName("IX_Vocabulary_WordAzerbaijani");
    }
}