using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Translator.Domain.DataModels;
using Translator.Infrastructure.Configurations;
using Translator.Infrastructure.Database.Postgres.SeedData;

namespace Translator.Infrastructure.Database.Postgres;

public class ApplicationDbContext : DbContext
{
    private readonly IOptions<LanguageSeedingConfiguration> _languageSeedingConfiguration;
    public DbSet<Template> Templates { get; set; }
    public DbSet<Language> Languages { get; set; }
    public DbSet<Translation> Translations { get; set; }
    public DbSet<Value> Values { get; set; }
    public DbSet<Category> Categories { get; set; }
    
    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        IOptions<LanguageSeedingConfiguration> languageSeedingConfiguration) 
        : base(options)
    {
        _languageSeedingConfiguration = languageSeedingConfiguration;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        _ = new LanguageSeeder(_languageSeedingConfiguration.Value).Seed(modelBuilder);
    }
}