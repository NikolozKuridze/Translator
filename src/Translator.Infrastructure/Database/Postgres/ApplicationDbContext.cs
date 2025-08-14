using Microsoft.EntityFrameworkCore;
using Translator.Domain.DataModels;
using Translator.Infrastructure.Database.Postgres.SeedData;

namespace Translator.Infrastructure.Database.Postgres;

public class ApplicationDbContext : DbContext
{
    public DbSet<Template> Templates { get; set; }
    public DbSet<Language> Languages { get; set; }
    public DbSet<Translation> Translations { get; set; }
    public DbSet<Value> Values { get; set; }
    public DbSet<Category> Categories { get; set; }
    
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) 
        : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        LanguageSeeder.Seed(modelBuilder);
    }
}