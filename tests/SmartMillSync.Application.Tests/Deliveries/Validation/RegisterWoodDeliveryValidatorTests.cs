using SmartMillSync.Application.Deliveries.Commands;
using SmartMillSync.Application.Deliveries.Validation;
using SmartMillSync.Shared.DTOs;
using Xunit;

namespace SmartMillSync.Application.Tests.Deliveries.Validation;

public sealed class RegisterWoodDeliveryValidatorTests
{
    private readonly RegisterWoodDeliveryValidator _validator = new();

    [Theory]
    [InlineData("ABC1234")]
    [InlineData("ABC-1234")]
    [InlineData("ABC1D23")]
    [InlineData("ABC-1D23")]
    public async Task Validate_WithSupportedPlate_IsValid(string plate)
    {
        var result = await _validator.ValidateAsync(CreateCommand(plate));

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("AB12345")]
    [InlineData("ABC-12D3")]
    [InlineData("")]
    public async Task Validate_WithInvalidPlate_IsInvalid(string plate)
    {
        var result = await _validator.ValidateAsync(CreateCommand(plate));

        Assert.Contains(result.Errors, error => error.PropertyName.EndsWith("TruckPlate"));
    }

    [Theory]
    [InlineData(0, 0, 45)]
    [InlineData(50, 50, 45)]
    [InlineData(101, 10, 45)]
    [InlineData(50, 10, 9)]
    [InlineData(50, 10, 71)]
    public async Task Validate_WithInvalidMeasurements_IsInvalid(
        decimal grossWeight,
        decimal tareWeight,
        decimal moisture)
    {
        var request = new CreateWoodDeliveryRequest(
            "ABC1D23", "Origin", "Species", grossWeight, tareWeight, moisture);

        var result = await _validator.ValidateAsync(new RegisterWoodDeliveryCommand(request));

        Assert.False(result.IsValid);
    }

    private static RegisterWoodDeliveryCommand CreateCommand(string plate) => new(
        new CreateWoodDeliveryRequest(plate, "Origin", "Species", 50m, 10m, 45m));
}
