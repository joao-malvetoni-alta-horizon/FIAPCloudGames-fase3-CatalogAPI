using System.Runtime.CompilerServices;

namespace CatalogAPI.API.Tests;

internal static class TestEnvironmentInitializer
{
    [ModuleInitializer]
    internal static void Initialize() =>
        Environment.SetEnvironmentVariable("TESTCONTAINERS_RYUK_DISABLED", "true");
}