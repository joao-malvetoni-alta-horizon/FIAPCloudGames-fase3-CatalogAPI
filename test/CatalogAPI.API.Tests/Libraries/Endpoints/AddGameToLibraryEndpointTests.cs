using System.Net;
using System.Net.Http.Json;
using CatalogAPI.API.Tests.Fixtures;

namespace CatalogAPI.API.Tests.Libraries.Endpoints;

[Collection("Api")]
public class AddGameToLibraryEndpointTests : IAsyncLifetime
{
    private readonly ApiFactory _factory;

    public AddGameToLibraryEndpointTests(ApiFactory factory) => _factory = factory;

    public async Task InitializeAsync() => await _factory.ResetDatabaseAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Post_WithoutAuthentication_ShouldReturn401Unauthorized()
    {
        var client = _factory.CreateApiClient();

        var response = await client.PostAsJsonAsync("/api/v1/library/add", new { gameId = Guid.NewGuid() });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Post_WithExistingGameNotOwned_ShouldReturn202Accepted()
    {
        var game = await _factory.SeedGameAsync();
        var client = _factory.CreateAuthenticatedClient(Guid.NewGuid());

        var response = await client.PostAsJsonAsync("/api/v1/library/add", new { gameId = game.Id });

        Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
    }

    [Fact]
    public async Task Post_WithNonExistingGame_ShouldReturn404NotFound()
    {
        var client = _factory.CreateAuthenticatedClient(Guid.NewGuid());

        var response = await client.PostAsJsonAsync("/api/v1/library/add", new { gameId = Guid.NewGuid() });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Post_WhenUserAlreadyOwnsGame_ShouldReturn409Conflict()
    {
        var game = await _factory.SeedGameAsync();
        var userId = Guid.NewGuid();
        await _factory.SeedLibraryItemAsync(userId, game.Id);
        var client = _factory.CreateAuthenticatedClient(userId);

        var response = await client.PostAsJsonAsync("/api/v1/library/add", new { gameId = game.Id });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Post_WithEmptyGameId_ShouldReturn400BadRequest()
    {
        var client = _factory.CreateAuthenticatedClient(Guid.NewGuid());

        var response = await client.PostAsJsonAsync("/api/v1/library/add", new { gameId = Guid.Empty });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
