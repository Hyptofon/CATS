using Api.Dtos;
using FluentValidation;

namespace Api.Modules.Validators;

public class UpdateUserDtoValidator : AbstractValidator<UpdateUserDto>
{
    public UpdateUserDtoValidator()
    {
        RuleFor(x => x.FirstName)
            .MaximumLength(100)
            .WithMessage("First name must not exceed 100 characters");

        RuleFor(x => x.LastName)
            .MaximumLength(100)
            .WithMessage("Last name must not exceed 100 characters");

        RuleFor(x => x.MiddleName)
            .MaximumLength(100)
            .WithMessage("Middle name must not exceed 100 characters");

        RuleFor(x => x.Role)
            .IsInEnum()
            .WithMessage("Invalid role value")
            .When(x => x.Role.HasValue);
    }
}
