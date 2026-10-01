using Microsoft.Extensions.DependencyInjection;
using Project3D.Application.Services.Interfaces;

namespace Project3D.Application.Extensions;

/// <summary>
/// Registra os application services no DI container.
/// As implementações ficam na Infrastructure mas as interfaces estão no Application.
/// </summary>
public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Os tipos concretos são resolvidos por nome qualificado via Infrastructure
        // O registro é feito em InfrastructureServiceExtensions para evitar
        // dependência Application → Infrastructure
        return services;
    }
}
