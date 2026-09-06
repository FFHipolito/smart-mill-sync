using Bunit;
using Microsoft.AspNetCore.Components;
using SmartMillSync.Client.Layout;
using Xunit;

namespace SmartMillSync.Client.Tests.Layout;

public sealed class MainLayoutTests : TestContext
{
    [Fact]
    public void Render_ShowsOperationalShellAndBody()
    {
        RenderFragment body = builder => builder.AddContent(0, "Conteudo operacional");
        RenderFragment componentMarkup = builder =>
        {
            builder.OpenComponent<MainLayout>(0);
            builder.AddAttribute(1, nameof(MainLayout.Body), body);
            builder.CloseComponent();
        };

        var component = Render(componentMarkup).FindComponent<MainLayout>();

        Assert.Contains("Centro de controle de biomassa", component.Markup);
        Assert.Contains("Operacao ativa", component.Markup);
        Assert.Contains("Conteudo operacional", component.Markup);
    }
}
