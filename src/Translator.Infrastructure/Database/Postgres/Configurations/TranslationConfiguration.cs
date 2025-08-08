using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Translator.Domain.DataModels;
using Translator.Infrastructure.Database.Postgres.Constants;

namespace Translator.Infrastructure.Database.Postgres.Configurations;

public class TranslationConfiguration : IEntityTypeConfiguration<Translation>
{
    public void Configure(EntityTypeBuilder<Translation> builder)
    {
        builder
            .HasKey(x => x.Id);
        
        builder
            .HasQueryFilter(x => x.IsActive);
        
        builder
            .Property(x => x.TranslationValue)
            .IsRequired()
            .HasMaxLength(TranslationConstants.VALUE_MAX_LENGTH);
    }
}
