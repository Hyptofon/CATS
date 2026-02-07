using FluentValidation;

namespace Application.Containers.Commands;

public class UpdateContainerFillCommandValidator : AbstractValidator<UpdateContainerFillCommand>
{
    public UpdateContainerFillCommandValidator()
    {
        RuleFor(x => x.ContainerId)
            .GreaterThan(0).WithMessage("Container ID must be greater than 0");

        RuleFor(x => x.ProductId)
            .GreaterThan(0).WithMessage("Product ID must be greater than 0")
            .When(x => x.ProductId.HasValue);

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be greater than 0");

        RuleFor(x => x.Unit)
            .NotEmpty().WithMessage("Unit is required")
            .MaximumLength(50).WithMessage("Unit must not exceed 50 characters");

        RuleFor(x => x.ProductionDate)
            .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Production date cannot be in the future");

        RuleFor(x => x.ExpirationDate)
            .GreaterThan(x => x.ProductionDate).WithMessage("Expiration date must be after production date");
    }
}
