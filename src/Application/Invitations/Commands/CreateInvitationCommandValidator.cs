using FluentValidation;

namespace Application.Invitations.Commands;

public class CreateInvitationCommandValidator : AbstractValidator<CreateInvitationCommand>
{
    public CreateInvitationCommandValidator()
    {
        RuleFor(v => v.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(200);

        RuleFor(v => v.Role)
            .IsInEnum();
    }
}
