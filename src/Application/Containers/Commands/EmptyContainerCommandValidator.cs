using FluentValidation;

namespace Application.Containers.Commands;

public class EmptyContainerCommandValidator : AbstractValidator<EmptyContainerCommand>
{
    public EmptyContainerCommandValidator()
    {
        RuleFor(x => x.ContainerId)
            .GreaterThan(0).WithMessage("Container ID must be greater than 0");
    }
}
