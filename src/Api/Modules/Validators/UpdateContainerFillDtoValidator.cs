using Api.Dtos;
using FluentValidation;

namespace Api.Modules.Validators;

public class UpdateContainerFillDtoValidator : AbstractValidator<UpdateContainerFillDto>
{
    public UpdateContainerFillDtoValidator()
    {
        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be greater than 0");

        RuleFor(x => x.ProductionDate)
            .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Production date cannot be in the future");

        RuleFor(x => x.ExpirationDate)
            .GreaterThan(x => x.ProductionDate).WithMessage("Expiration date must be after production date");
    }
}
