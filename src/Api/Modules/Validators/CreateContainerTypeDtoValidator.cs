using Api.Dtos;
using FluentValidation;

namespace Api.Modules.Validators;

public class CreateContainerTypeDtoValidator : AbstractValidator<CreateContainerTypeDto>
{
    public CreateContainerTypeDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Container type name is required")
            .MaximumLength(100)
            .WithMessage("Container type name must not exceed 100 characters");

        RuleFor(x => x.DefaultUnit)
            .NotEmpty()
            .WithMessage("Default unit is required")
            .MaximumLength(20)
            .WithMessage("Default unit must not exceed 20 characters");
    }
}
