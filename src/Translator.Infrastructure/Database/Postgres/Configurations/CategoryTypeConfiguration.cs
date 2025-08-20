using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Translator.Domain;
using Translator.Domain.DataModels;
using Translator.Infrastructure.Database.Postgres.Configurations.Constants;

namespace Translator.Infrastructure.Database.Postgres.Configurations;

public class CategoryTypeConfiguration : IEntityTypeConfiguration<CategoryType>
{
    public void Configure(EntityTypeBuilder<CategoryType> builder)
    {
        builder.HasKey(ct => ct.Id);

        builder.Property(ct => ct.Name)
            .IsRequired()
            .HasMaxLength(DatabaseConstants.Type.NAME_MAX_LENGTH);

        builder.HasIndex(ct => ct.Name)
            .IsUnique();
    }
}