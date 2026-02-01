using Api.Dtos;
using FluentValidation;

namespace Api.Modules.Validators;

public class UpdateContainerTypeDtoValidator : AbstractValidator<UpdateContainerTypeDto>
{
    public UpdateContainerTypeDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Container type name is required")
            .MaximumLength(100)
            .WithMessage("Container type name must not exceed 100 characters");
    }
}