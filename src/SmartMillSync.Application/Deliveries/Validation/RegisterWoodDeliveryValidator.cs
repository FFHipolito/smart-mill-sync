using FluentValidation;
using SmartMillSync.Application.Deliveries.Commands;

namespace SmartMillSync.Application.Deliveries.Validation;

public sealed class RegisterWoodDeliveryValidator : AbstractValidator<RegisterWoodDeliveryCommand>
{
    public RegisterWoodDeliveryValidator()
    {
        RuleFor(command => command.Request).NotNull();

        When(command => command.Request is not null, () =>
        {
            RuleFor(command => command.Request.TruckPlate)
                .NotEmpty()
                .Matches("^[A-Za-z]{3}-?[0-9][A-Za-z0-9][0-9]{2}$");

            RuleFor(command => command.Request.ForestOrigin)
                .NotEmpty()
                .MaximumLength(160);

            RuleFor(command => command.Request.WoodSpecies)
                .NotEmpty()
                .MaximumLength(120);

            RuleFor(command => command.Request.GrossWeight)
                .GreaterThan(0m)
                .LessThanOrEqualTo(100m);

            RuleFor(command => command.Request.TareWeight)
                .GreaterThanOrEqualTo(0m)
                .LessThan(command => command.Request.GrossWeight);

            RuleFor(command => command.Request.MoisturePercentage)
                .InclusiveBetween(10m, 70m);
        });
    }
}
