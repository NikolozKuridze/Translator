using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Translator.Domain.DataModels;

namespace Translator.Infrastructure.Database.Postgres.Configurations;

public class TemplateValueConfiguration : IEntityTypeConfiguration<TemplateValue>
{
    public void Configure(EntityTypeBuilder<TemplateValue> templateValueBuilder)
    {
        templateValueBuilder
            .HasKey(x => x.Id);
        templateValueBuilder
            .HasIndex(x => x.Hash)
            .IsUnique();
        
        templateValueBuilder
            .Property(x => x.Key)
            .IsRequired();
        
        templateValueBuilder
            .HasOne(tv => tv.Template)
            .WithMany(t => t.TemplateValues)
            .HasForeignKey(tv => tv.TemplateId)
            .OnDelete(DeleteBehavior.Cascade);

        templateValueBuilder
            .HasMany(tv => tv.Translations)
            .WithOne(t => t.TemplateValue)
            .HasForeignKey(t => t.TemplateValueId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}