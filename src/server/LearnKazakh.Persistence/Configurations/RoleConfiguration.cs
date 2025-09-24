using LearnKazakh.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LearnKazakh.Persistence.Configurations;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        // Primary key
        builder.HasKey(r => r.Id);

        // Properties
        builder.Property(r => r.Name).IsRequired().HasMaxLength(100);
        builder.Property(r => r.Description).IsRequired().HasMaxLength(500);

        // Indexes
        builder
            .HasIndex(r => r.Name)
            .IsUnique()
            .HasDatabaseName("IX_Roles_Name");
    }
}