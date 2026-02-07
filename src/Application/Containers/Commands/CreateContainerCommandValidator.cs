using FluentValidation;

namespace Application.Containers.Commands;

public class CreateContainerCommandValidator : AbstractValidator<CreateContainerCommand>
{
    public CreateContainerCommandValidator()
    {
        RuleFor(x => x.Code)
            .MaximumLength(50);

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Volume)
            .GreaterThan(0);

        RuleFor(x => x.Unit)
            .NotEmpty()
            .MaximumLength(20);

        RuleFor(x => x.ContainerTypeId)
            .NotEmpty();
    }
}
