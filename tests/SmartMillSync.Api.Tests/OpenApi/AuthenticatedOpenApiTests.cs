using Microsoft.OpenApi.Models;
using SmartMillSync.Api.OpenApi;
using Xunit;

namespace SmartMillSync.Api.Tests.OpenApi;

public sealed class AuthenticatedOpenApiTests
{
    [Fact]
    public void ErrorFilter_WhenAuthenticationIsEnabled_AddsUnauthorizedAndForbidden()
    {
        var operation = new OpenApiOperation
        {
            Responses = new OpenApiResponses()
        };
        var filter = new StandardErrorResponsesOperationFilter(true);

        filter.Apply(operation, null!);

        Assert.Contains("401", operation.Responses.Keys);
        Assert.Contains("403", operation.Responses.Keys);
        Assert.All(new[] { "401", "403" }, status =>
            Assert.Contains("application/problem+json", operation.Responses[status].Content.Keys));
    }
}
