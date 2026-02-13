using Api.Dtos;
using FluentValidation;

namespace Api.Modules.Validators;

public class CreateUserDtoValidator : AbstractValidator<CreateUserDto>
{
    public CreateUserDtoValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email is required")
            .EmailAddress()
            .WithMessage("Email is not valid")
            .MaximumLength(200)
            .WithMessage("Email must not exceed 200 characters");

        RuleFor(x => x.FirstName)
            .NotEmpty()
            .WithMessage("First name is required")
            .MaximumLength(100)
            .WithMessage("First name must not exceed 100 characters");

        RuleFor(x => x.LastName)
            .NotEmpty()
            .WithMessage("Last name is required")
            .MaximumLength(100)
            .WithMessage("Last name must not exceed 100 characters");

        RuleFor(x => x.MiddleName)
            .MaximumLength(100)
            .WithMessage("Middle name must not exceed 100 characters");

        RuleFor(x => x.Role)
            .IsInEnum()
            .WithMessage("Invalid role value");
    }
}
