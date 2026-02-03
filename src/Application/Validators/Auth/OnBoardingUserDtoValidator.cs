using Application.DTOs.Auth;
using FluentValidation;

namespace Application.Validators.Auth;

public class OnBoardingUserDtoValidator : AbstractValidator<OnboardingUserDto>
{
    public OnBoardingUserDtoValidator()
    {
        RuleFor(user => user.Email)
            .NotEmpty().WithMessage("Email is required")
            .Matches(@"^[^@\s]+@[^@\s]+\.[^@\s]+$").WithMessage("Invalid email format.")
            .MaximumLength(100).WithMessage("Email cannot exceed 100 characters");
        
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
        // .MustAsync(async (id, cancellationToken) =>
        // {
        //     if (id is null) return true;
        //     return await roleService.ExistsAsync(id, cancellationToken);
        // })
        // .WithMessage("Role not found!");
    }
}