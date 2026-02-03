using Application.DTOs.Accounts;
using FluentValidation;

namespace Application.Validators.Accounts;

public class AccountTypeCreateDtoValidator : AbstractValidator<AccountTypeCreateDto>
{
    public AccountTypeCreateDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Account type name is required.")
            .MaximumLength(15).WithMessage("Account type name must not exceed 15 characters.");
    }
}
