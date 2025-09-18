using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Translator.Domain.Entities;
using Translator.Infrastructure.Database.Postgres.Configurations.Constants;

namespace Translator.Infrastructure.Database.Postgres.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);

        builder.Property(u => u.Username)
            .IsRequired()
            .HasMaxLength(DatabaseConstants.User.USERNAME_MAX_LENGTH);
        builder.HasIndex(u => u.Username)
            .IsUnique();
        
        builder.Property(u => u.SecretKey)
            .IsRequired()
            .HasMaxLength(DatabaseConstants.User.SECRET_KEY_LENGTH);
        builder.HasIndex(u => u.SecretKey)
            .IsUnique();
        
        builder.HasMany(u => u.Templates)
            .WithOne(t => t.Owner)
            .HasForeignKey(t => t.OwnerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.Values)
            .WithOne(v => v.Owner)
            .HasForeignKey(v => v.OwnerId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}