using FluentValidation;

namespace Application.Users.Commands;

public class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserCommandValidator()
    {
        RuleFor(v => v.UserId)
            .NotEmpty();

        RuleFor(v => v.FirstName)
            .MaximumLength(100);

        RuleFor(v => v.LastName)
            .MaximumLength(100);

        RuleFor(v => v.MiddleName)
            .MaximumLength(100);

        RuleFor(v => v.Role)
            .IsInEnum()
            .When(v => v.Role.HasValue);
    }
}
