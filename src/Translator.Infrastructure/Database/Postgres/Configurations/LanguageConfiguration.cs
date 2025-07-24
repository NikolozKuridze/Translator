using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Translator.Domain.DataModels;
using Translator.Infrastructure.Database.Postgres.Constants;

namespace Translator.Infrastructure.Database.Postgres.Configurations;

public class LanguageConfiguration : IEntityTypeConfiguration<Language>
{
    public void Configure(EntityTypeBuilder<Language> builder)
    {
        builder
            .HasKey(l => l.Id);
        builder
            .HasIndex(l => l.Code)
            .IsUnique();

        builder
            .Property(l => l.Code)
            .IsRequired()
            .HasMaxLength(LanguageConstants.CODE_MAX_LENGTH);

        builder
            .Property(l => l.Name)
            .IsRequired()
            .HasMaxLength(LanguageConstants.NAME_MAX_LENGTH);

        builder
            .Property(l => l.UnicodeRange)
            .IsRequired()
            .HasMaxLength(LanguageConstants.UNICDDE_MAX_LENGTH);

        builder
            .Property(l => l.IsActive)
            .HasDefaultValue(true);

        builder
            .HasMany(l => l.Translations)
            .WithOne(t => t.Language)
            .HasForeignKey(t => t.LanguageId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}