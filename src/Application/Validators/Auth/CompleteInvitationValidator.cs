using Application.DTOs.Auth;
using FluentValidation;

namespace Application.Validators.Auth;

public class CompleteInvitationValidator : AbstractValidator<SetupPasswordRequest>
{
    public CompleteInvitationValidator()
    {
        RuleFor(e => e.Email)
            .NotEmpty().WithMessage("Email is required.")
            .Matches(@"^[^@\s]+@[^@\s]+\.[^@\s]+$").WithMessage("Invalid email format.")
            .MaximumLength(100).WithMessage("Email cannot exceed 100 characters");
        
        RuleFor(e => e.UserId)
            .NotEmpty().WithMessage("User ID is required.")
            .MaximumLength(50).WithMessage("User id must not exceed 50 characters.");
        
        RuleFor(e => e.NewPassword)
            .NotEmpty().WithMessage("New password is required.")
            .MinimumLength(8).WithMessage("New password must be at least 8 characters long.");

        RuleFor(e => e.ConfirmPassword)
            .NotEmpty().WithMessage("Confirm password is required.")
            .MinimumLength(8)
            .WithMessage("Confirm password must be at least 8 characters long.")
            .Equal(x => x.NewPassword).WithMessage("Confirm password must match the new password!");
    }
}