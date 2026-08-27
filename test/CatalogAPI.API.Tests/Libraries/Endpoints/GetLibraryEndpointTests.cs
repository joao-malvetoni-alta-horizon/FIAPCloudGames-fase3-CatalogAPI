using System.Net;
using System.Text.Json;
using CatalogAPI.API.Tests.Fixtures;
using CatalogAPI.Domain.Contexts.Games.Enums;

namespace CatalogAPI.API.Tests.Libraries.Endpoints;

[Collection("Api")]
public class GetLibraryEndpointTests : IAsyncLifetime
{
    private readonly ApiFactory _factory;

    public GetLibraryEndpointTests(ApiFactory factory) => _factory = factory;

    public async Task InitializeAsync() => await _factory.ResetDatabaseAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    private async Task SeedOwnedGameAsync(Guid userId, string title)
    {
        var game = await _factory.SeedGameAsync(title, "A description for the game", 19.99m, GameGenre.Action);
        await _factory.SeedLibraryItemAsync(userId, game.Id);
    }

    [Fact]
    public async Task Get_WithoutAuthentication_ShouldReturn401Unauthorized()
    {
        var client = _factory.CreateApiClient();

        var response = await client.GetAsync("/api/v1/library");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Get_WhenLibraryIsEmpty_ShouldReturn200WithEmptyList()
    {
        var client = _factory.CreateAuthenticatedClient(Guid.NewGuid());

        var response = await client.GetAsync("/api/v1/library");
        var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var library = body.RootElement.GetProperty("library");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(0, library.GetProperty("total").GetInt32());
        Assert.Equal(0, library.GetProperty("games").GetArrayLength());
    }

    [Fact]
    public async Task Get_WhenUserOwnsGames_ShouldReturn200WithGames()
    {
        var userId = Guid.NewGuid();
        await SeedOwnedGameAsync(userId, "First Game Title");
        await SeedOwnedGameAsync(userId, "Second Game Title");
        var client = _factory.CreateAuthenticatedClient(userId);

        var response = await client.GetAsync("/api/v1/library");
        var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var library = body.RootElement.GetProperty("library");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(2, library.GetProperty("total").GetInt32());
        Assert.Equal(2, library.GetProperty("games").GetArrayLength());
    }

    [Fact]
    public async Task Get_ShouldReturnOnlyTheAuthenticatedUsersGames()
    {
        var userId = Guid.NewGuid();
        await SeedOwnedGameAsync(userId, "My Game Title");
        await SeedOwnedGameAsync(Guid.NewGuid(), "Someone Elses Game Title");
        var client = _factory.CreateAuthenticatedClient(userId);

        var response = await client.GetAsync("/api/v1/library");
        var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var library = body.RootElement.GetProperty("library");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(1, library.GetProperty("total").GetInt32());
        Assert.Equal(1, library.GetProperty("games").GetArrayLength());
    }

    [Fact]
    public async Task Get_WithZeroPage_ShouldReturn400BadRequest()
    {
        var client = _factory.CreateAuthenticatedClient(Guid.NewGuid());

        var response = await client.GetAsync("/api/v1/library?page=0");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Get_WithPageSizeGreaterThan50_ShouldReturn400BadRequest()
    {
        var client = _factory.CreateAuthenticatedClient(Guid.NewGuid());

        var response = await client.GetAsync("/api/v1/library?pageSize=51");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
