using CatalogAPI.Application.Shared.Cache;
using CatalogAPI.Domain.Contexts.Games.Commands;
using CatalogAPI.Domain.Contexts.Games.Queries;
using CatalogAPI.Infrastructure.Cache;
using CatalogAPI.Infrastructure.Contexts.Games.UseCases.Create;
using CatalogAPI.Infrastructure.Contexts.Libraries.Messaging;
using CatalogAPI.Infrastructure.Data;
using FiapCloudGames.Contracts.Payments;
using FiapCloudGames.RabbitMq.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace CatalogAPI.Infrastructure.DependencyInjection;

public static class InfrastructureServiceExtension
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")!;

        services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));

        services.AddScoped<
            ICreate,
            Repository>();

        services.AddScoped<
            IGetAll,
            Contexts.Games.UseCases.GetAll.Repository>();

        services.AddScoped<
            IGetById,
            Contexts.Games.UseCases.GetById.Repository>();

        services.AddScoped<
            IDelete,
            Contexts.Games.UseCases.Delete.Repository>();

        services.AddScoped<
            IUpdate,
            Contexts.Games.UseCases.Update.Repository>();

        services.AddScoped<
            Domain.Contexts.Libraries.Queries.IGetGameAndCheckOwnership,
            Contexts.Libraries.UseCases.InitiateGamePurchase.Repository>();

        services.AddScoped<
            Domain.Contexts.Libraries.Commands.IGamePurchase,
            Contexts.Libraries.UseCases.GamePurchase.Repository>();

        services.AddScoped<
            Domain.Contexts.Libraries.Queries.IGetLibrary,
            Contexts.Libraries.UseCases.GetLibrary.Repository>();

        AddMessaging(services, configuration);

        AddRedis(services, configuration);

        return services;
    }

    private static void AddMessaging(IServiceCollection services, IConfiguration configuration)
    {
        services.AddRabbitMq(configuration);

        services.AddSingleton<
            Application.Shared.Messaging.IIntegrationEventPublisher,
            RabbitMqIntegrationEventPublisher>();

        services.AddSingleton<
            Application.Shared.Messaging.IEventDispatcher,
            EventDispatcher>();

        services.AddRabbitMqConsumer<PaymentProcessedMessageProcessor>(
            new(
                PaymentsMessaging.Exchange,
                "catalog.payment-processed",
                PaymentsMessaging.RoutingKeys.Status));
    }
    private static void AddRedis(IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IConnectionMultiplexer>(sp =>
            ConnectionMultiplexer.Connect(configuration.GetConnectionString("Redis")));

        services.AddSingleton<ICacheService, RedisCacheService>();
    }
}