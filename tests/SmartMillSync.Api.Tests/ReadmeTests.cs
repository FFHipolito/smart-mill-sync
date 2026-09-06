using Xunit;

namespace SmartMillSync.Api.Tests;

public sealed class ReadmeTests
{
    [Fact]
    public void Readme_DocumentsPurposeArchitectureSetupSecurityAndTests()
    {
        var readmePath = Path.Combine(FindRepositoryRoot(), "README.md");
        var content = File.ReadAllText(readmePath);

        Assert.Contains("## Para que serve", content);
        Assert.Contains("## Como foi desenvolvido", content);
        Assert.Contains("## Stack utilizada", content);
        Assert.Contains("## Como executar localmente", content);
        Assert.Contains("## Seguranca", content);
        Assert.Contains("dotnet user-secrets set", content);
        Assert.Contains("docker compose up -d postgres", content);
        Assert.Contains("dotnet test SmartMillSync.sln", content);
        Assert.DoesNotContain("AQ.", content, StringComparison.Ordinal);
        Assert.DoesNotContain("AIza", content, StringComparison.Ordinal);
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
