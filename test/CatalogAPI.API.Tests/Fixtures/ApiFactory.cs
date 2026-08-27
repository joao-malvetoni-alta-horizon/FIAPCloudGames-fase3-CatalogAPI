using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using CatalogAPI.Domain.Contexts.Games.Entities;
using CatalogAPI.Domain.Contexts.Games.Enums;
using CatalogAPI.Domain.Contexts.Libraries.Entities;
using CatalogAPI.Application.Shared.Messaging;
using CatalogAPI.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Testcontainers.PostgreSql;

namespace CatalogAPI.API.Tests.Fixtures;

public class ApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    // Same key the API uses to validate the JWT signature (mirrors UsersAPI's shared secret).
    public const string TestSecretKey = "6a8a56f4d31a272d6e2f048f710c9cce45b51aa343fd6f73d8b69f1218eaac56";

    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:18")
        .WithImagePullPolicy(_ => false)
        .WithDatabase("catalogdb_apitest")
        .WithUsername("postgres")
        .WithPassword("pass")
        .Build();

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();
    }

    public new async Task DisposeAsync()
    {
        await _container.DisposeAsync();
        await base.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration(config =>
        {
            // Pin the signing key so the API validates tokens minted by this factory.
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["JwtSettings:SecretKey"] = TestSecretKey
            });
        });

        builder.ConfigureServices(services =>
        {
            // Replace the DbContext registration so it points to the test container
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.RemoveAll<AppDbContext>();

            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(_container.GetConnectionString()));

            // Endpoint tests must not depend on a real RabbitMQ broker:
            // drop all hosted services (including the RabbitMQ consumer) and stub the event publisher.
            services.RemoveAll<IHostedService>();
            services.RemoveAll<IIntegrationEventPublisher>();
            services.AddSingleton<IIntegrationEventPublisher, NoOpEventPublisher>();
        });
    }

    public HttpClient CreateApiClient() => CreateClient(new WebApplicationFactoryClientOptions
    {
        AllowAutoRedirect = false
    });

    public HttpClient CreateAuthenticatedClient(Guid userId) =>
        CreateClientWithToken(CreateJwtFor(userId, TestSecretKey));

    public HttpClient CreateClientWithToken(string token)
    {
        var client = CreateApiClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    // Builds a compact HS256 JWT signed with <paramref name="secretKey"/>. Tests can pass a wrong
    // key or a past expiry to exercise the signature/lifetime validation added in BuilderExtension.
    public static string CreateJwtFor(Guid userId, string secretKey, DateTimeOffset? expires = null)
    {
        var exp = (expires ?? DateTimeOffset.UtcNow.AddHours(1)).ToUnixTimeSeconds();
        var header = Base64Url("{\"alg\":\"HS256\",\"typ\":\"JWT\"}");
        var payload = Base64Url($"{{\"sub\":\"{userId}\",\"exp\":{exp}}}");
        var signingInput = $"{header}.{payload}";

        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secretKey));
        var signature = Base64Url(hmac.ComputeHash(Encoding.UTF8.GetBytes(signingInput)));
        return $"{signingInput}.{signature}";
    }

    private static string Base64Url(string text) =>
        Convert.ToBase64String(Encoding.UTF8.GetBytes(text))
            .TrimEnd('=').Replace('+', '-').Replace('/', '_');

    private static string Base64Url(byte[] bytes) =>
        Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');

    public async Task ResetDatabaseAsync()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.LibraryItems.ExecuteDeleteAsync();
        await db.Games.ExecuteDeleteAsync();
    }

    public async Task SeedLibraryItemAsync(Guid userId, Guid gameId)
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.LibraryItems.Add(new LibraryItem(userId, gameId));
        await db.SaveChangesAsync();
    }

    private sealed class NoOpEventPublisher : IIntegrationEventPublisher
    {
        public Task PublishAsync<TEvent>(
            TEvent integrationEvent,
            CancellationToken cancellationToken = default)
            where TEvent : FiapCloudGames.Contracts.IIntegrationEvent
            => Task.CompletedTask;
    }

    public async Task<Game> SeedGameAsync(
        string title = "Game Title Test",
        string description = "A description for the game",
        decimal price = 49.99m,
        GameGenre genre = GameGenre.Action,
        DateOnly? releaseDate = null)
    {
        var date = releaseDate ?? DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));
        var game = new Game(title, description, price, genre, date);

        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Games.Add(game);
        await db.SaveChangesAsync();

        return game;
    }
}

[CollectionDefinition("Api")]
public class ApiCollection : ICollectionFixture<ApiFactory> { }
