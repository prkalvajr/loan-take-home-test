using Loans.Application.Rules;
using Loans.Application.Rules.DenyRules;
using Microsoft.Extensions.DependencyInjection;

namespace Loans.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Deny rules: adding a rule means adding a class and one line here.
        services.AddScoped<IDenyRule, NewYorkStateRule>();
        services.AddScoped<IDenyRule, SsnBlacklistRule>();
        services.AddScoped<RuleEngine>();

        return services;
    }
}
