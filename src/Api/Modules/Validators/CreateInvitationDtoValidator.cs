using Api.Dtos;
using FluentValidation;

namespace Api.Modules.Validators;

public class CreateInvitationDtoValidator : AbstractValidator<CreateInvitationDto>
{
    public CreateInvitationDtoValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email is required")
            .EmailAddress()
            .WithMessage("Email is not valid")
            .MaximumLength(200)
            .WithMessage("Email must not exceed 200 characters");

        RuleFor(x => x.Role)
            .IsInEnum()
            .WithMessage("Invalid role value");
    }
}
