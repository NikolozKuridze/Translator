using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Translator.Domain.DataModels;
using Translator.Infrastructure.Database.Postgres.Configurations.Constants;

namespace Translator.Infrastructure.Database.Postgres.Configurations;

public class TranslationConfiguration : IEntityTypeConfiguration<Translation>
{
    public void Configure(EntityTypeBuilder<Translation> builder)
    {
        builder
            .HasKey(x => x.Id);
        
        builder
            .Property(x => x.TranslationValue)
            .IsRequired()
            .HasMaxLength(DatabaseConstants.Translation.VALUE_MAX_LENGTH);
    }
}
