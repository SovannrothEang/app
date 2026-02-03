using Application.DTOs.Auth;
using FluentValidation;

namespace Application.Validators.Auth;

public class RoleCreateDtoValidator : AbstractValidator<RoleCreateDto>
{
    public RoleCreateDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Role name is required.")
            .Length(5, 100).WithMessage("Role name must be between 5 and 100 characters.");
    }
}
