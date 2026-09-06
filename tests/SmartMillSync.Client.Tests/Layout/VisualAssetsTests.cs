using Xunit;

namespace SmartMillSync.Client.Tests.Layout;

public sealed class VisualAssetsTests
{
    [Fact]
    public void Index_IsLocalizedAndDoesNotLoadBootstrap()
    {
        var content = File.ReadAllText(ClientPath("wwwroot", "index.html"));

        Assert.Contains("lang=\"pt-BR\"", content);
        Assert.DoesNotContain("bootstrap", content, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Smart Mill Sync", content);
    }

    [Theory]
    [InlineData("Layout/MainLayout.razor.css")]
    [InlineData("Layout/NavMenu.razor.css")]
    public void LayoutStyles_ContainMobileBreakpoint(string relativePath)
    {
        var content = File.ReadAllText(ClientPath(relativePath.Split('/')));

        Assert.Contains("@media (max-width: 760px)", content);
    }

    [Fact]
    public void GlobalStyles_DefineIndustrialColorTokens()
    {
        var content = File.ReadAllText(ClientPath("wwwroot", "css", "app.css"));

        Assert.Contains("--green: #55c878", content);
        Assert.Contains("--amber: #e7ad52", content);
        Assert.Contains("--red: #e5675f", content);
    }

    private static string ClientPath(params string[] segments) => Path.Combine(
        FindRepositoryRoot(),
        "src",
        "SmartMillSync.Client",
        Path.Combine(segments));

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
