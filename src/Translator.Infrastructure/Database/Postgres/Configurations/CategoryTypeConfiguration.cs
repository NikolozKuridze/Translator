using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Translator.Domain;
using Translator.Domain.Entities;

namespace Translator.Infrastructure.Database.Postgres.Configurations;

public class CategoryTypeConfiguration : IEntityTypeConfiguration<CategoryType>
{
    public void Configure(EntityTypeBuilder<CategoryType> builder)
    {
        builder.HasKey(ct => ct.Id);

        builder.Property(ct => ct.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(ct => ct.Name)
            .IsUnique();
    }
}