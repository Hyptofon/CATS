using FluentValidation;

namespace Application.ProductTypes.Commands;

public class CreateProductTypeCommandValidator : AbstractValidator<CreateProductTypeCommand>
{
    public CreateProductTypeCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.ShelfLifeDays)
            .GreaterThanOrEqualTo(0)
            .When(x => x.ShelfLifeDays.HasValue);
    }
}