using DynamicCodeExecutionAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace DynamicCodeExecutionAPI.Contexts;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<SourceCode> SourceCodes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SourceCode>().ToTable(nameof(SourceCode));
    }

}
