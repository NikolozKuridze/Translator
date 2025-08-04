using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Translator.Domain.DataModels;

namespace Translator.Infrastructure.Database.Postgres.Configurations;

public class ValueConfiguration : IEntityTypeConfiguration<Value>
{
    public void Configure(EntityTypeBuilder<Value> templateValueBuilder)
    {
        templateValueBuilder
            .HasKey(x => x.Id);
        templateValueBuilder
            .HasIndex(x => x.Hash)
            .IsUnique();
        
        templateValueBuilder
            .Property(x => x.Key)
            .IsRequired()
            .HasMaxLength(100);
        templateValueBuilder
            .Property(x => x.Hash)
            .HasMaxLength(100);

        templateValueBuilder
            .HasMany(tv => tv.Translations)
            .WithOne(t => t.Value)
            .HasForeignKey(t => t.TemplateValueId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}