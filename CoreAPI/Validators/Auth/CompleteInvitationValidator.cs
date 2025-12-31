using CoreAPI.DTOs.Auth;
using FluentValidation;

namespace CoreAPI.Validators.Auth;

public class CompleteInvitationValidator : AbstractValidator<SetupPasswordRequest>
{
    public CompleteInvitationValidator()
    {
        RuleFor(e => e.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email address.");
        
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
            .Must((model, confirmPassword) => confirmPassword == model.NewPassword)
            .WithMessage("Passwords do not match.");
    }
}