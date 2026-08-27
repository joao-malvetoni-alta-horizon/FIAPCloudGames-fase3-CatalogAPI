using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using CatalogAPI.API.Tests.Fixtures;

namespace CatalogAPI.API.Tests.Games.Endpoints;

[Collection("Api")]
public class UpdateGameEndpointTests : IAsyncLifetime
{
    private readonly ApiFactory _factory;
    private readonly HttpClient _client;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    private static readonly DateOnly Tomorrow = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));

    public UpdateGameEndpointTests(ApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateApiClient();
    }

    public async Task InitializeAsync() => await _factory.ResetDatabaseAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Put_WithExistingGame_ShouldReturn204NoContent()
    {
        var seeded = await _factory.SeedGameAsync();
        var body = new { title = "Updated Title", description = "Updated description text here", price = 59.99 };

        var response = await _client.PutAsJsonAsync($"/api/v1/games/{seeded.Id}", body, JsonOptions);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Put_WithNonExistingId_ShouldReturn404NotFound()
    {
        var body = new { title = "Updated Title" };

        var response = await _client.PutAsJsonAsync($"/api/v1/games/{Guid.NewGuid()}", body, JsonOptions);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Put_WithEmptyGuid_ShouldReturn400BadRequest()
    {
        var body = new { title = "Updated Title" };

        var response = await _client.PutAsJsonAsync($"/api/v1/games/{Guid.Empty}", body, JsonOptions);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Put_WithNegativePrice_ShouldReturn400BadRequest()
    {
        var seeded = await _factory.SeedGameAsync();
        var body = new { price = -1.0 };

        var response = await _client.PutAsJsonAsync($"/api/v1/games/{seeded.Id}", body, JsonOptions);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Put_WithNullBody_ShouldReturn204NoContent()
    {
        var seeded = await _factory.SeedGameAsync();
        var body = new { };

        var response = await _client.PutAsJsonAsync($"/api/v1/games/{seeded.Id}", body, JsonOptions);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Put_ShouldPersistChanges()
    {
        var seeded = await _factory.SeedGameAsync();
        var body = new { title = "Verified Updated Title", price = 99.99 };

        await _client.PutAsJsonAsync($"/api/v1/games/{seeded.Id}", body, JsonOptions);

        var getResponse = await _client.GetAsync($"/api/v1/games/{seeded.Id}");
        var getBody = JsonDocument.Parse(await getResponse.Content.ReadAsStringAsync());
        var game = getBody.RootElement.GetProperty("game");

        Assert.Equal("Verified Updated Title", game.GetProperty("title").GetString());
        Assert.Equal(99.99m, game.GetProperty("price").GetDecimal());
    }
}
