using Fundo.Domain.Entities;
using Fundo.Infrastructure.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;

namespace Fundo.Infrastructure.Persistence;

public class FundoDbContext : DbContext
{
    public FundoDbContext(DbContextOptions<FundoDbContext> options) : base(options)
    {
    }

    public DbSet<Loan> Loans => Set<Loan>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new LoanConfiguration());
        base.OnModelCreating(modelBuilder);
    }
}
