using Api.Dtos;
using FluentValidation;

namespace Api.Modules.Validators;

public class CreateProductDtoValidator : AbstractValidator<CreateProductDto>
{
    public CreateProductDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Product name is required")
            .MaximumLength(200).WithMessage("Product name must not exceed 200 characters");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description must not exceed 500 characters");

        RuleFor(x => x.ProductTypeId)
            .GreaterThan(0).WithMessage("Product type ID must be greater than 0");

        RuleFor(x => x.ShelfLifeDays)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Shelf life days cannot be negative")
            .When(x => x.ShelfLifeDays.HasValue);

        RuleFor(x => x.ShelfLifeHours)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Shelf life hours cannot be negative")
            .When(x => x.ShelfLifeHours.HasValue);
    }
}
