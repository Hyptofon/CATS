using FluentValidation;

namespace Application.Containers.Commands;

public class DeleteContainerCommandValidator : AbstractValidator<DeleteContainerCommand>
{
    public DeleteContainerCommandValidator()
    {
        RuleFor(x => x.ContainerId).GreaterThan(0);
    }
}