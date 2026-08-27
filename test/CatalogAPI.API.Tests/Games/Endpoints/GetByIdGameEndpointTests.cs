using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using CatalogAPI.API.Tests.Fixtures;

namespace CatalogAPI.API.Tests.Games.Endpoints;

[Collection("Api")]
public class GetByIdGameEndpointTests : IAsyncLifetime
{
    private readonly ApiFactory _factory;
    private readonly HttpClient _client;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public GetByIdGameEndpointTests(ApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateApiClient();
    }

    public async Task InitializeAsync() => await _factory.ResetDatabaseAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task GetById_WithExistingId_ShouldReturn200WithGameDetail()
    {
        var seeded = await _factory.SeedGameAsync();

        var response = await _client.GetAsync($"/api/v1/games/{seeded.Id}");
        var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var game = body.RootElement.GetProperty("game");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(seeded.Id, game.GetProperty("id").GetGuid());
        Assert.Equal(seeded.Title.Value, game.GetProperty("title").GetString());
        Assert.Equal(seeded.Price.Amount, game.GetProperty("price").GetDecimal());
        Assert.Equal(seeded.Description, game.GetProperty("description").GetString());
        Assert.Equal("Active", game.GetProperty("status").GetString());
    }

    [Fact]
    public async Task GetById_WithNonExistingId_ShouldReturn404NotFound()
    {
        var response = await _client.GetAsync($"/api/v1/games/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetById_WithEmptyGuid_ShouldReturn400BadRequest()
    {
        var response = await _client.GetAsync($"/api/v1/games/{Guid.Empty}");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
