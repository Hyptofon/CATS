using FluentValidation;

namespace Application.ContainerTypes.Commands;

public class CreateContainerTypeCommandValidator : AbstractValidator<CreateContainerTypeCommand>
{
    public CreateContainerTypeCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.DefaultUnit)
            .NotEmpty()
            .MaximumLength(20);
    }
}
