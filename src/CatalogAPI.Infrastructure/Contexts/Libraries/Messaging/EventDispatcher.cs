using CatalogAPI.Application.Shared.Messaging;
using Microsoft.Extensions.DependencyInjection;

namespace CatalogAPI.Infrastructure.Contexts.Libraries.Messaging;

/// <summary>
/// Único ponto do projeto que conhece o contêiner de injeção de dependência para resolver
/// <see cref="IEventHandler{TEvent}"/>. Cria um escopo de DI por evento despachado, já que os
/// handlers dependem de serviços <c>Scoped</c> (ex.: o repositório e o <see cref="AppDbContext"/>).
/// </summary>
public class EventDispatcher(IServiceScopeFactory scopeFactory) : IEventDispatcher
{
    public async Task DispatchAsync<TEvent>(TEvent integrationEvent, CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var handler = scope.ServiceProvider.GetRequiredService<IEventHandler<TEvent>>();
        await handler.HandleAsync(integrationEvent, cancellationToken);
    }
}