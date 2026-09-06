using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using SmartMillSync.Client.Layout;
using Xunit;

namespace SmartMillSync.Client.Tests.Layout;

public sealed class NavMenuTests : TestContext
{
    [Fact]
    public void Render_ExposesOnlyOperationalNavigation()
    {
        Services.AddSingleton<NavigationManager>(new TestNavigationManager());
        RenderFragment componentMarkup = builder =>
        {
            builder.OpenComponent<NavMenu>(0);
            builder.CloseComponent();
        };

        var component = Render(componentMarkup).FindComponent<NavMenu>();
        var links = component.FindAll("nav a");

        Assert.Single(links);
        Assert.Contains("Patio de madeira", links[0].TextContent);
        Assert.DoesNotContain("Counter", component.Markup);
        Assert.DoesNotContain("Weather", component.Markup);
    }

    private sealed class TestNavigationManager : NavigationManager
    {
        public TestNavigationManager() => Initialize("http://localhost/", "http://localhost/");

        protected override void NavigateToCore(string uri, NavigationOptions options)
        {
        }
    }
}
