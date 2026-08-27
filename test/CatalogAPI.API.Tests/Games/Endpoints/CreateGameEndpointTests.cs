using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using CatalogAPI.API.Tests.Fixtures;

namespace CatalogAPI.API.Tests.Games.Endpoints;

[Collection("Api")]
public class CreateGameEndpointTests : IAsyncLifetime
{
    private readonly ApiFactory _factory;
    private readonly HttpClient _client;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    private static readonly DateOnly Tomorrow = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));

    public CreateGameEndpointTests(ApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateApiClient();
    }

    public async Task InitializeAsync() => await _factory.ResetDatabaseAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    private object ValidBody() => new
    {
        title = "Game Title Test",
        description = "A valid description for the game",
        price = 49.99,
        genre = "Action",
        releaseDate = Tomorrow.ToString("yyyy-MM-dd")
    };

    [Fact]
    public async Task Post_WithValidBody_ShouldReturn201Created()
    {
        var response = await _client.PostAsJsonAsync("/api/v1/games", ValidBody(), JsonOptions);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task Post_WithValidBody_ShouldReturnGameInBody()
    {
        var response = await _client.PostAsJsonAsync("/api/v1/games", ValidBody(), JsonOptions);
        var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var game = body.RootElement.GetProperty("game");

        Assert.Equal("Game Title Test", game.GetProperty("title").GetString());
        Assert.Equal(49.99m, game.GetProperty("price").GetDecimal());
        Assert.Equal("Action", game.GetProperty("genre").GetString());
        Assert.NotEqual(Guid.Empty, game.GetProperty("id").GetGuid());
    }

    [Fact]
    public async Task Post_WithValidBody_ShouldReturnLocationHeader()
    {
        var response = await _client.PostAsJsonAsync("/api/v1/games", ValidBody(), JsonOptions);

        Assert.NotNull(response.Headers.Location);
        Assert.Contains("/api/v1/games/", response.Headers.Location.ToString());
    }

    [Fact]
    public async Task Post_WithEmptyTitle_ShouldReturn400BadRequest()
    {
        var body = new { title = "", description = "A valid description for the game", price = 10.0, genre = "Action", releaseDate = Tomorrow.ToString("yyyy-MM-dd") };

        var response = await _client.PostAsJsonAsync("/api/v1/games", body, JsonOptions);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Post_WithTitleTooShort_ShouldReturn400BadRequest()
    {
        var body = new { title = "ShortTitle", description = "A valid description for the game", price = 10.0, genre = "Action", releaseDate = Tomorrow.ToString("yyyy-MM-dd") };

        var response = await _client.PostAsJsonAsync("/api/v1/games", body, JsonOptions);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Post_WithNegativePrice_ShouldReturn400BadRequest()
    {
        var body = new { title = "Game Title Test", description = "A valid description for the game", price = -1.0, genre = "Action", releaseDate = Tomorrow.ToString("yyyy-MM-dd") };

        var response = await _client.PostAsJsonAsync("/api/v1/games", body, JsonOptions);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Post_WithPastReleaseDate_ShouldReturn400BadRequest()
    {
        var yesterday = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)).ToString("yyyy-MM-dd");
        var body = new { title = "Game Title Test", description = "A valid description for the game", price = 10.0, genre = "Action", releaseDate = yesterday };

        var response = await _client.PostAsJsonAsync("/api/v1/games", body, JsonOptions);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Post_WithZeroPrice_ShouldReturn201Created()
    {
        var body = new { title = "Game Title Test", description = "A valid description for the game", price = 0.0, genre = "Action", releaseDate = Tomorrow.ToString("yyyy-MM-dd") };

        var response = await _client.PostAsJsonAsync("/api/v1/games", body, JsonOptions);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }
}
