using Xunit;

namespace SmartMillSync.Client.Tests.Components;

public sealed class IndustrialCopilotChatStylesTests
{
    [Fact]
    public void Styles_AnchorPanelAndConstrainDesktopAndMobileDimensions()
    {
        var path = Path.Combine(
            FindRepositoryRoot(),
            "src",
            "SmartMillSync.Client",
            "Components",
            "IndustrialCopilotChat.razor.css");
        var content = File.ReadAllText(path);

        Assert.Contains("position: fixed", content);
        Assert.Contains("right: 22px", content);
        Assert.Contains("height: min(560px", content);
        Assert.Contains("@media (max-width: 620px)", content);
        Assert.Contains(".markdown-content ::deep strong", content);
        Assert.Contains("table-layout: fixed", content);
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
