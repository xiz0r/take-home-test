using Fundo.Application.UseCases.Create;
using Fundo.Application.UseCases.Find;
using Fundo.Application.UseCases.Payment;
using Microsoft.Extensions.DependencyInjection;

namespace Fundo.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<LoanCreator>();
        services.AddScoped<LoanFinder>();
        services.AddScoped<LoanListFinder>();
        services.AddScoped<LoanPaymentMaker>();
        return services;
    }
}
