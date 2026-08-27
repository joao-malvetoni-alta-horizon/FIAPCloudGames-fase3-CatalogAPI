using System.Runtime.CompilerServices;

namespace CatalogAPI.Infrastructure.Tests;

internal static class TestEnvironmentInitializer
{
    // Disables the Ryuk resource reaper to avoid Docker Hub manifest auth checks.
    // Container cleanup is handled by IAsyncLifetime.DisposeAsync in DatabaseFixture.
    [ModuleInitializer]
    internal static void Initialize() =>
        Environment.SetEnvironmentVariable("TESTCONTAINERS_RYUK_DISABLED", "true");
}