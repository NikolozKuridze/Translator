using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Translator.Infrastructure.Database.Postgres;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions options) 
        : base(options) { }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    { } 
}