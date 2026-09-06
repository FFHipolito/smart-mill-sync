using FluentValidation;
using MediatR;
using SmartMillSync.Application.Behaviors;
using Xunit;

namespace SmartMillSync.Application.Tests.Behaviors;

public sealed class ValidationBehaviorTests
{
    [Fact]
    public async Task Handle_WithValidRequest_InvokesNextHandler()
    {
        var behavior = new ValidationBehavior<TestRequest, string>(
            new[] { new TestRequestValidator() });

        var result = await behavior.Handle(
            new TestRequest("valid"),
            _ => Task.FromResult("handled"),
            CancellationToken.None);

        Assert.Equal("handled", result);
    }

    [Fact]
    public async Task Handle_WithInvalidRequest_ThrowsValidationException()
    {
        var behavior = new ValidationBehavior<TestRequest, string>(
            new[] { new TestRequestValidator() });

        await Assert.ThrowsAsync<ValidationException>(() => behavior.Handle(
            new TestRequest(string.Empty),
            _ => Task.FromResult("handled"),
            CancellationToken.None));
    }

    private sealed record TestRequest(string Value);

    private sealed class TestRequestValidator : AbstractValidator<TestRequest>
    {
        public TestRequestValidator() => RuleFor(request => request.Value).NotEmpty();
    }
}
