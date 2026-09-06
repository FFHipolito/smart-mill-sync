using Xunit;

namespace SmartMillSync.Api.Tests;

public sealed class ComposeConfigurationTests
{
    [Fact]
    public void Compose_DefinesPersistentHealthyPostgreSqlService()
    {
        var composePath = Path.Combine(FindRepositoryRoot(), "compose.yaml");
        var content = File.ReadAllText(composePath);

        Assert.Contains("image: postgres:16-alpine", content);
        Assert.Contains("5432:5432", content);
        Assert.Contains("pg_isready", content);
        Assert.Contains("smart-mill-sync-postgres:/var/lib/postgresql/data", content);
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "SmartMillSync.sln")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName ?? throw new DirectoryNotFoundException("Repository root not found.");
    }
}
