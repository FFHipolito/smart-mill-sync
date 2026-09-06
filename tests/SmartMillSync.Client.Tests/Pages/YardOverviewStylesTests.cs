using Xunit;

namespace SmartMillSync.Client.Tests.Pages;

public sealed class YardOverviewStylesTests
{
    [Fact]
    public void Styles_DefineStableResponsiveKpiAndTableLayouts()
    {
        var path = Path.Combine(
            FindRepositoryRoot(),
            "src",
            "SmartMillSync.Client",
            "Pages",
            "YardOverview.razor.css");
        var content = File.ReadAllText(path);

        Assert.Contains("grid-template-columns: repeat(4", content);
        Assert.Contains("table-layout: fixed", content);
        Assert.Contains("@media (max-width: 620px)", content);
        Assert.Contains(".modal-backdrop", content);
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
