using FluentValidation;

namespace Application.Users.Commands;

public class UpdateProfileCommandValidator : AbstractValidator<UpdateProfileCommand>
{
    public UpdateProfileCommandValidator()
    {
        RuleFor(v => v.FirstName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(v => v.LastName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(v => v.MiddleName)
            .MaximumLength(100);
    }
}
