using System.Reflection;
using CatalogAPI.Application.Contexts.Libraries.EventHandlers;
using CatalogAPI.Application.Shared.Messaging;
using FiapCloudGames.Contracts.Payments;
using Microsoft.Extensions.DependencyInjection;

namespace CatalogAPI.Application.DependencyInjection;

public static class ApplicationServiceExtension
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

        services.AddScoped<IEventHandler<PaymentProcessedEvent>, PaymentProcessedEventHandler>();

        return services;
    }
}