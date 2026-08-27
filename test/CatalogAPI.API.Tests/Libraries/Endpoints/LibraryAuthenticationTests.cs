using System.Net;
using CatalogAPI.API.Tests.Fixtures;

namespace CatalogAPI.API.Tests.Libraries.Endpoints;

// Guards the JWT signature/lifetime validation configured in BuilderExtension.AddBearerAuthentication:
// the API must reject any token it cannot prove was issued by the UsersAPI (shared secret key).
[Collection("Api")]
public class LibraryAuthenticationTests : IAsyncLifetime
{
    private const string Endpoint = "/api/v1/library";

    private readonly ApiFactory _factory;

    public LibraryAuthenticationTests(ApiFactory factory) => _factory = factory;

    public async Task InitializeAsync() => await _factory.ResetDatabaseAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Get_WithTokenSignedByAnotherKey_ShouldReturn401Unauthorized()
    {
        var forged = ApiFactory.CreateJwtFor(Guid.NewGuid(), secretKey: "an-attacker-controlled-key-not-the-shared-secret");
        var client = _factory.CreateClientWithToken(forged);

        var response = await client.GetAsync(Endpoint);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Get_WithExpiredToken_ShouldReturn401Unauthorized()
    {
        var expired = ApiFactory.CreateJwtFor(
            Guid.NewGuid(), ApiFactory.TestSecretKey, expires: DateTimeOffset.UtcNow.AddMinutes(-5));
        var client = _factory.CreateClientWithToken(expired);

        var response = await client.GetAsync(Endpoint);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Get_WithTamperedSignature_ShouldReturn401Unauthorized()
    {
        var valid = ApiFactory.CreateJwtFor(Guid.NewGuid(), ApiFactory.TestSecretKey);
        var tampered = valid[..^2] + (valid[^1] == 'a' ? 'b' : 'a'); // flip the last signature char
        var client = _factory.CreateClientWithToken(tampered);

        var response = await client.GetAsync(Endpoint);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Get_WithValidToken_ShouldPassAuthentication()
    {
        var client = _factory.CreateAuthenticatedClient(Guid.NewGuid());

        var response = await client.GetAsync(Endpoint);

        // A properly signed, unexpired token must clear authentication (empty library => 200 OK).
        Assert.NotEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
