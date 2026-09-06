using FluentValidation;
using SmartMillSync.Shared.DTOs;

namespace SmartMillSync.Application.Agents;

public sealed class IndustrialAgentChatValidator : AbstractValidator<IndustrialAgentChatRequest>
{
    public const int MaximumMessageLength = 2_000;
    public const int MaximumHistoryMessages = 20;
    public const int MaximumHistoryMessageLength = 4_000;
    public const int MaximumHistoryLength = 20_000;

    public IndustrialAgentChatValidator()
    {
        RuleFor(request => request.Message)
            .NotEmpty()
            .MaximumLength(MaximumMessageLength);

        RuleFor(request => request.History)
            .Cascade(CascadeMode.Stop)
            .NotNull();

        When(request => request.History is not null, () =>
        {
            RuleFor(request => request.History)
                .Must(history => history.Count <= MaximumHistoryMessages)
                .WithMessage($"History cannot contain more than {MaximumHistoryMessages} messages.")
                .Must(history => history.Sum(message => message.Content?.Length ?? 0) <= MaximumHistoryLength)
                .WithMessage($"History cannot exceed {MaximumHistoryLength} characters.");

            RuleForEach(request => request.History).ChildRules(message =>
            {
                message.RuleFor(item => item.Role).IsInEnum();
                message.RuleFor(item => item.Content)
                    .NotEmpty()
                    .MaximumLength(MaximumHistoryMessageLength);
            });
        });
    }
}
