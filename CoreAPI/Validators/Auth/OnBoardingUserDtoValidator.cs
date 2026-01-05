using CoreAPI.DTOs.Auth;
using FluentValidation;

namespace CoreAPI.Validators.Auth;

public class OnBoardingUserDtoValidator : AbstractValidator<OnboardingUserDto>
{
    public OnBoardingUserDtoValidator()
    {
        RuleFor(user => user.Email)
            .NotEmpty().WithMessage("Email is required")
            .MaximumLength(100).WithMessage("Email cannot exceed 100 characters")
            .EmailAddress().WithMessage("Invalid email address.");
        
        RuleFor(user => user.UserName)
            .NotEmpty().WithMessage("Username is required")
            .MaximumLength(50).WithMessage("Username cannot exceed 50 characters");
        
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("FirstName is required")
            .MaximumLength(50).WithMessage("FirstName cannot exceed 50 characters");
        
        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("LastName is required")
            .MaximumLength(50).WithMessage("LastName cannot exceed 50 characters");
        
        RuleFor(user => user.Role)
            .MaximumLength(50).WithMessage("Role cannot exceed 50 characters");
    }    
}