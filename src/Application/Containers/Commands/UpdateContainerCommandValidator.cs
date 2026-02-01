using FluentValidation;

namespace Application.Containers.Commands;

public class UpdateContainerCommandValidator : AbstractValidator<UpdateContainerCommand>
{
    public UpdateContainerCommandValidator()
    {
        RuleFor(x => x.ContainerId).NotEmpty();

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Volume)
            .GreaterThan(0);

        RuleFor(x => x.ContainerTypeId)
            .NotEmpty();
    }
}