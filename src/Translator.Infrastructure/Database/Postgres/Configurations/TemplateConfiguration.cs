using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.ValueGeneration.Internal;
using Translator.Domain.Entities;
using Translator.Infrastructure.Database.Postgres.Configurations.Constants;

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
            .HasMaxLength(DatabaseConstants.Template.TEMPLATE_HASH_MAX_LENGTH);

        templateBuilder
            .Property(x => x.Name)
            .HasMaxLength(DatabaseConstants.Template.TEMPLATE_NAME_MAX_LENGTH);

        templateBuilder
            .HasMany(x => x.Values)
            .WithMany(x => x.Templates)
            .UsingEntity(x => x.ToTable("TemplateValues"));
        
        templateBuilder.HasOne(t => t.Owner)
            .WithMany(t => t.Templates)
            .HasForeignKey(t => t.OwnerId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}