using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrgMembershipService.Application.Database;
using OrgMembershipService.Application.Users.Services;
using OrgMembershipService.Infrastructure.Database;
using OrgMembershipService.Infrastructure.Identity;

namespace OrgMembershipService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options
            => options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));
        services.AddScoped<IDbContext>(sp => sp.GetRequiredService<AppDbContext>());
        
        services.AddScoped<IIdentityProviderService, IdentityProviderService>();
        
        services.Configure<KeycloakOptions>(configuration.GetSection("Keycloak"));
        
        services.AddHttpClient<IKeycloakTokenClient, KeycloakTokenClient>();
        
        services.AddTransient<KeycloakAuthDelegatingHandler>();
        
        services
            .AddHttpClient<KeycloakClient>((serviceProvider, httpClient) =>
            {
                var keycloakOptions = serviceProvider.GetRequiredService<IOptions<KeycloakOptions>>().Value;
                httpClient.BaseAddress = new Uri(keycloakOptions.AdminUrl);
            })
            .AddHttpMessageHandler<KeycloakAuthDelegatingHandler>();
        
        return services;
    }
}