using Api.Dtos;
using FluentValidation;

namespace Api.Modules.Validators;

public class UpdateProductTypeDtoValidator : AbstractValidator<UpdateProductTypeDto>
{
    public UpdateProductTypeDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Product type name is required")
            .MaximumLength(100)
            .WithMessage("Product type name must not exceed 100 characters");

        RuleFor(x => x.ShelfLifeDays)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Shelf life days cannot be negative")
            .When(x => x.ShelfLifeDays.HasValue);
    }
}