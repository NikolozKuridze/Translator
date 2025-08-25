using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Translator.Domain.Entities;
using Translator.Infrastructure.Database.Postgres.Configurations.Constants;

namespace Translator.Infrastructure.Database.Postgres.Configurations;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.HasKey(c => c.Id);
        
        builder.Property(c => c.Value)
            .IsRequired()
            .HasMaxLength(DatabaseConstants.Category.VALUE_MAX_LENGTH);

        builder.Property(c => c.TypeId)
            .IsRequired();

        builder.Property(c => c.Metadata)
            .HasColumnType("jsonb");

        builder.HasIndex(c => c.Shortcode);
        
        builder.Property(c => c.Shortcode)
            .HasMaxLength(DatabaseConstants.Category.SHORT_CODE_MAX_LENGTH);

        builder.Property(c => c.Order);

        builder.HasOne(c => c.Type)
            .WithMany(ct => ct.Categories)
            .HasForeignKey(c => c.TypeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(c => c.Parent)
            .WithMany(c => c.Children)
            .HasForeignKey(c => c.ParentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}