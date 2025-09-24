using LearnKazakh.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LearnKazakh.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        // Primary key
        builder.HasKey(u => u.Id);

        // Properties
        builder.Property(u => u.Email).IsRequired().HasMaxLength(254); // RFC Standard
        builder.Property(u => u.PasswordHash).IsRequired().HasMaxLength(255);
        builder.Property(u => u.PasswordSalt).IsRequired().HasMaxLength(255);
        builder.Property(u => u.FirstName).IsRequired().HasMaxLength(100);
        builder.Property(u => u.LastName).IsRequired().HasMaxLength(100);
        builder.Property(u => u.PhoneNumber).HasMaxLength(20);
        builder.Property(u => u.Status).IsRequired();
        builder.Property(u => u.EmailVerified).IsRequired();

        // Relationships
        builder
            .HasMany(u => u.RefreshTokens)
            .WithOne(u => u.User)
            .HasForeignKey(rt => rt.UserId);

        // Indexes
        builder
            .HasIndex(u => u.Email)
            .IsUnique()
            .HasDatabaseName("IX_Users_Email");
    }
}