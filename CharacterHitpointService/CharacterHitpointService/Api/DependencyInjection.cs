using CharacterHitpointService.Api.AddTemporaryHitpoints;
using CharacterHitpointService.Api.Damage;
using CharacterHitpointService.Api.Heal;
using CharacterHitpointService.Util;

namespace CharacterHitpointService.Api;

public static class DependencyInjection
{
    public static IServiceCollection AddApiEndpoints(this IServiceCollection services)
    {
        services.AddTransient<IEndpoint, DamageCharacterEndpoint>();
        services.AddTransient<IEndpoint, HealCharacterEndpoint>();
        services.AddTransient<IEndpoint, AddTemporaryHitpointsEndpoint>();
        return services;
    }

    public static IApplicationBuilder MapApiEndpoints(this WebApplication app)
    {
        var endpoints = app.Services.GetServices<IEndpoint>();

        foreach (var endpoint in endpoints)
        {
            endpoint.MapEndpoint(app);
        }

        return app;
    }
}