using Fundo.Application;
using Fundo.Domain.Repositories;
using Fundo.Infrastructure.Persistence;
using Fundo.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Fundo.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        Action<DbContextOptionsBuilder> configureDb)
    {
        services.AddDbContext<FundoDbContext>(options =>
            configureDb(options));

        services.AddScoped<ILoanRepository, LoanRepository>();
        
        services.AddApplication();

        return services;
    }

    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        return services.AddInfrastructure(options => options.UseSqlServer(connectionString));
    }
}
