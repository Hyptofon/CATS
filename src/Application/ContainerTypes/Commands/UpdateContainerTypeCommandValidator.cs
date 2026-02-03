using FluentValidation;

namespace Application.ContainerTypes.Commands;

public class UpdateContainerTypeCommandValidator : AbstractValidator<UpdateContainerTypeCommand>
{
    public UpdateContainerTypeCommandValidator()
    {
        RuleFor(x => x.ContainerTypeId).GreaterThan(0);
        
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);
    }
}