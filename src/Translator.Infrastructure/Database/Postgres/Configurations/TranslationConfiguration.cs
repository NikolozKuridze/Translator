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
            .Property(x => x.Language)
            .HasConversion<string>()
            .IsRequired();
        
        builder.Property(x => x.Key)
            .IsRequired()
            .HasMaxLength(TranslationConstants.KEY_MAX_LENGTH);

        builder
            .HasOne(x => x.TemplateValue)
            .WithMany()
            .HasForeignKey(x => x.TemplateValueId);
    }
}
