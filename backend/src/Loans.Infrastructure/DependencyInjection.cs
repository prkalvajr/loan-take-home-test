using Loans.Application.Abstractions;
using Loans.Infrastructure.Blacklist;
using Loans.Infrastructure.Persistence;
using Loans.Infrastructure.Persistence.Interceptors;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Loans.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<AuditTimestampsInterceptor>();

        services.AddDbContext<LoansDbContext>((provider, options) => options
            .UseNpgsql(connectionString)
            .UseSnakeCaseNamingConvention()
            .AddInterceptors(provider.GetRequiredService<AuditTimestampsInterceptor>()));

        // The DbContext is the unit of work, so the repository and outbox share its transaction.
        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<LoansDbContext>());
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<ISsnBlacklist, SsnBlacklist>();

        return services;
    }
}
