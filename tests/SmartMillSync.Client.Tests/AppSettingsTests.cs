using System.Text.Json;
using Xunit;

namespace SmartMillSync.Client.Tests;

public sealed class AppSettingsTests
{
    [Fact]
    public void AppSettings_ContainsAbsoluteApiBaseUrl()
    {
        var path = Path.Combine(
            FindRepositoryRoot(),
            "src",
            "SmartMillSync.Client",
            "wwwroot",
            "appsettings.json");
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        var value = document.RootElement.GetProperty("ApiBaseUrl").GetString();

        Assert.True(Uri.TryCreate(value, UriKind.Absolute, out _));
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
