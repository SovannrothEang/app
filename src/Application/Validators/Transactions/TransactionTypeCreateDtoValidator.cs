using Application.DTOs.Transactions;
using FluentValidation;

namespace Application.Validators.Transactions;

public class TransactionTypeCreateDtoValidator : AbstractValidator<TransactionTypeCreateDto>
{
    public TransactionTypeCreateDtoValidator()
    {
       RuleFor(x => x.Slug)
            .NotEmpty().WithMessage("Slug is required.")
            .MaximumLength(15).WithMessage("Slug must not exceed 100 characters.");
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(15).WithMessage("Name must not exceed 200 characters.");
        RuleFor(x => x.Description)
            .MaximumLength(255).WithMessage("Description must not exceed 500 characters.");
        RuleFor(x => x.Multiplier)
            .GreaterThan(0).WithMessage("Multiplier must be greater than zero.");
        RuleFor(x => x.AllowNegative)
            .NotNull().WithMessage("AllowNegative must be specified.");

    }
}
