using CoreAPI.DTOs.Auth;
using FluentValidation;

namespace CoreAPI.Validators.Auth;

public class SetupPasswordRequestValidator : AbstractValidator<SetupPasswordRequest>
{
    public SetupPasswordRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required!")
            .Matches(@"^[^@\s]+@[^@\s]+\.[^@\s]+$").WithMessage("Invalid email format.")
            .MaximumLength(100).WithMessage("Email cannot exceed 100 characters");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("New password is required!")
            .MinimumLength(8).WithMessage("New password must be at least 8 characters long!");

        RuleFor(x => x.ConfirmPassword)
            .NotEmpty().WithMessage("Confirm password is required!")
            .Equal(x => x.NewPassword).WithMessage("Confirm password must match the new password!");
    }
}
