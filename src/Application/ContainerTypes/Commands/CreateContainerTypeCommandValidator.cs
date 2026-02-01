using FluentValidation;

namespace Application.ContainerTypes.Commands;

public class CreateContainerTypeCommandValidator : AbstractValidator<CreateContainerTypeCommand>
{
    public CreateContainerTypeCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);
    }
}