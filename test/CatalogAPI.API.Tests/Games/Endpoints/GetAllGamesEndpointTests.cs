using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using CatalogAPI.API.Tests.Fixtures;
using CatalogAPI.Domain.Contexts.Games.Enums;

namespace CatalogAPI.API.Tests.Games.Endpoints;

[Collection("Api")]
public class GetAllGamesEndpointTests : IAsyncLifetime
{
    private readonly ApiFactory _factory;
    private readonly HttpClient _client;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public GetAllGamesEndpointTests(ApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateApiClient();
    }

    public async Task InitializeAsync() => await _factory.ResetDatabaseAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Get_WhenDatabaseIsEmpty_ShouldReturn200WithEmptyList()
    {
        var response = await _client.GetAsync("/api/v1/games");
        var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var data = body.RootElement.GetProperty("data");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(0, data.GetProperty("total").GetInt32());
        Assert.Equal(0, data.GetProperty("games").GetArrayLength());
    }

    [Fact]
    public async Task Get_WhenDatabaseHasGames_ShouldReturn200WithGames()
    {
        await _factory.SeedGameAsync("First Game Title", "A description for the first game", 10m, GameGenre.Action);
        await _factory.SeedGameAsync("Second Game Title", "A description for the second game", 20m, GameGenre.RPG);

        var response = await _client.GetAsync("/api/v1/games");
        var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var data = body.RootElement.GetProperty("data");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(2, data.GetProperty("total").GetInt32());
        Assert.Equal(2, data.GetProperty("games").GetArrayLength());
    }

    [Fact]
    public async Task Get_WithDefaultPagination_ShouldReturnPage1With20PageSize()
    {
        var response = await _client.GetAsync("/api/v1/games");
        var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var data = body.RootElement.GetProperty("data");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(1, data.GetProperty("page").GetInt32());
        Assert.Equal(20, data.GetProperty("pageSize").GetInt32());
    }

    [Fact]
    public async Task Get_WithCustomPagination_ShouldReturnCorrectPage()
    {
        for (var i = 1; i <= 5; i++)
            await _factory.SeedGameAsync($"Game Title Number {i:D2}", $"Description for game number {i}", i * 10m, GameGenre.Action);

        var response = await _client.GetAsync("/api/v1/games?page=2&pageSize=2");
        var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var data = body.RootElement.GetProperty("data");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(5, data.GetProperty("total").GetInt32());
        Assert.Equal(2, data.GetProperty("page").GetInt32());
        Assert.Equal(2, data.GetProperty("pageSize").GetInt32());
        Assert.Equal(2, data.GetProperty("games").GetArrayLength());
    }

    [Fact]
    public async Task Get_WithZeroPage_ShouldReturn400BadRequest()
    {
        var response = await _client.GetAsync("/api/v1/games?page=0");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Get_WithPageSizeGreaterThan50_ShouldReturn400BadRequest()
    {
        var response = await _client.GetAsync("/api/v1/games?pageSize=51");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
