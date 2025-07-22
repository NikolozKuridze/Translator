using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
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
            .Property(x => x.Hash)
            .HasMaxLength(TemplateConstants.TEMPLATE_HASH_MAX_LENGTH);

        templateBuilder
            .HasMany(x => x.TemplateValues)
            .WithOne(x => x.Template)
            .HasForeignKey(x => x.TemplateId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}