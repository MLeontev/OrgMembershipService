using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using OrgMembershipService.Application.Behaviors;
using OrgMembershipService.Application.Services;

namespace OrgMembershipService.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddUseCases(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        });
        
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly, includeInternalTypes: true);
        
        services.AddScoped<IUserIdentityResolver, UserIdentityResolver>();
        services.AddScoped<IAccessPolicyService, AccessPolicyService>();

        return services;
    }
}
