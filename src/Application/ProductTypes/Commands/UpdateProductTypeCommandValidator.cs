using FluentValidation;

namespace Application.ProductTypes.Commands;

public class UpdateProductTypeCommandValidator : AbstractValidator<UpdateProductTypeCommand>
{
    public UpdateProductTypeCommandValidator()
    {
        RuleFor(x => x.ProductTypeId).NotEmpty();
        
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.ShelfLifeDays)
            .GreaterThanOrEqualTo(0)
            .When(x => x.ShelfLifeDays.HasValue);
    }
}