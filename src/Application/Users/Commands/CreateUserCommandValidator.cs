using FluentValidation;

namespace Application.Users.Commands;

public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(v => v.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(200);

        RuleFor(v => v.FirstName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(v => v.LastName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(v => v.MiddleName)
            .MaximumLength(100);

        RuleFor(v => v.Role)
            .IsInEnum();
    }
}
