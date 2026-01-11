using Fundo.Domain.Entities;
using Fundo.Domain.Enums;
using Fundo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Fundo.Infrastructure.Persistence;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<FundoDbContext>();
        
        await context.Database.EnsureCreatedAsync();

        if (await context.Loans.AnyAsync())
            return;

        var loans = GetSeedLoans();
        await context.Loans.AddRangeAsync(loans);
        await context.SaveChangesAsync();
    }

    private static List<Loan> GetSeedLoans()
    {
        // Using reflection to set properties since domain entity uses private setters
        var loans = new List<Loan>();

        var loanData = new[]
        {
            new { Amount = 1500.00m, CurrentBalance = 500.00m, Name = "Maria Silva", Status = LoanStatus.Active },
            new { Amount = 5000.00m, CurrentBalance = 5000.00m, Name = "John Doe", Status = LoanStatus.Active },
            new { Amount = 2500.00m, CurrentBalance = 0.00m, Name = "Jane Smith", Status = LoanStatus.Paid },
            new { Amount = 10000.00m, CurrentBalance = 7500.00m, Name = "Carlos Rodriguez", Status = LoanStatus.Active },
            new { Amount = 3000.00m, CurrentBalance = 1200.00m, Name = "Ana Garcia", Status = LoanStatus.Active },
            new { Amount = 8000.00m, CurrentBalance = 0.00m, Name = "Pedro Martinez", Status = LoanStatus.Paid },
            new { Amount = 4500.00m, CurrentBalance = 4500.00m, Name = "Laura Fernandez", Status = LoanStatus.Active },
            new { Amount = 6000.00m, CurrentBalance = 2000.00m, Name = "Miguel Lopez", Status = LoanStatus.Active },
        };

        foreach (var data in loanData)
        {
            var loan = Loan.Create(data.Amount, data.Name);
            
            // Make payments to adjust the balance
            if (data.CurrentBalance < data.Amount)
            {
                var paymentAmount = data.Amount - data.CurrentBalance;
                loan.MakePayment(paymentAmount);
            }

            loans.Add(loan);
        }

        return loans;
    }
}
