using System.Net;
using CatalogAPI.API.Tests.Fixtures;

namespace CatalogAPI.API.Tests.Games.Endpoints;

[Collection("Api")]
public class DeleteGameEndpointTests : IAsyncLifetime
{
    private readonly ApiFactory _factory;
    private readonly HttpClient _client;

    public DeleteGameEndpointTests(ApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateApiClient();
    }

    public async Task InitializeAsync() => await _factory.ResetDatabaseAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Delete_WithExistingId_ShouldReturn204NoContent()
    {
        var seeded = await _factory.SeedGameAsync();

        var response = await _client.DeleteAsync($"/api/v1/games/{seeded.Id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Delete_WithNonExistingId_ShouldReturn404NotFound()
    {
        var response = await _client.DeleteAsync($"/api/v1/games/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Delete_WithEmptyGuid_ShouldReturn400BadRequest()
    {
        var response = await _client.DeleteAsync($"/api/v1/games/{Guid.Empty}");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Delete_ShouldRemoveGameFromDatabase()
    {
        var seeded = await _factory.SeedGameAsync();

        await _client.DeleteAsync($"/api/v1/games/{seeded.Id}");

        var getResponse = await _client.GetAsync($"/api/v1/games/{seeded.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task Delete_ShouldOnlyRemoveTargetGame()
    {
        var target = await _factory.SeedGameAsync("Target Game Title", "Description for the target game");
        var other = await _factory.SeedGameAsync("Other Game Title", "Description for the other game");

        await _client.DeleteAsync($"/api/v1/games/{target.Id}");

        var getResponse = await _client.GetAsync($"/api/v1/games/{other.Id}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
    }
}
