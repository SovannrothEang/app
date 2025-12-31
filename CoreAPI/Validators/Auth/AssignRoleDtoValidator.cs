using CoreAPI.DTOs.Auth;
using FluentValidation;

namespace CoreAPI.Validators.Auth;

public class AssignRoleDtoValidator : AbstractValidator<AssignRoleDto>
{
    public AssignRoleDtoValidator()
    {
        RuleFor(e => e.UserName)
            .NotEmpty().WithMessage("Username is required.")
            .Length(3, 100).WithMessage("Username must be between 3 and 100 characters long.");
        
        RuleFor(e => e.RoleName)
            .NotEmpty().WithMessage("Role name is required.")
            .MaximumLength(100).WithMessage("Role name must exceed 100 characters long.");
    }
}