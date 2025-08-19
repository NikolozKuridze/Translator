using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Translator.Domain;
using Translator.Domain.DataModels;

namespace Translator.Infrastructure.Database.Postgres.Configurations;

public class CategoryTypeConfiguration : IEntityTypeConfiguration<CategoryType>
{
    public void Configure(EntityTypeBuilder<CategoryType> builder)
    {
        builder.HasKey(ct => ct.Id);

        builder.Property(ct => ct.Type)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(ct => ct.Type)
            .IsUnique();
    }
}