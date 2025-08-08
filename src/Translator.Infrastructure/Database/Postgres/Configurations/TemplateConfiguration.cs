using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.ValueGeneration.Internal;
using Translator.Domain.DataModels;
using Translator.Infrastructure.Database.Postgres.Constants;

namespace Translator.Infrastructure.Database.Postgres.Configurations;

public class TemplateConfiguration : IEntityTypeConfiguration<Template>
{
    public void Configure(EntityTypeBuilder<Template> templateBuilder)
    {
        templateBuilder
            .HasKey(x => x.Id);
        templateBuilder
            .HasIndex(x => x.Hash)
            .IsUnique();
        
        templateBuilder
            .HasQueryFilter(x => x.IsActive);
        
        templateBuilder
            .Property(x => x.Hash)
            .HasMaxLength(TemplateConstants.TEMPLATE_HASH_MAX_LENGTH);

        templateBuilder
            .Property(x => x.Name)
            .HasMaxLength(TemplateConstants.TEMPLATE_NAME_MAX_LENGTH);

        templateBuilder
            .HasMany(x => x.Values)
            .WithMany(x => x.Templates)
            .UsingEntity(x => x.ToTable("TemplateValues"));
    }
}