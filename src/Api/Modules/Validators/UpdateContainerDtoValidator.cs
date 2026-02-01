using Api.Dtos;
using FluentValidation;

namespace Api.Modules.Validators;

public class UpdateContainerDtoValidator : AbstractValidator<UpdateContainerDto>
{
    public UpdateContainerDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Container name is required")
            .MaximumLength(100)
            .WithMessage("Container name must not exceed 100 characters");

        RuleFor(x => x.Volume)
            .GreaterThan(0)
            .WithMessage("Volume must be greater than 0");

        RuleFor(x => x.ContainerTypeId)
            .NotEmpty()
            .WithMessage("Container type is required");
    }
}