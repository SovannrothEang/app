using CoreAPI.DTOs.Customers;
using FluentValidation;

namespace CoreAPI.Validators.Customers;

public class CustomerAdjustPointDtoValidator : AbstractValidator<CustomerAdjustPointDto>
{
    public CustomerAdjustPointDtoValidator()
    {
        RuleFor(x => x.Amount)
            .NotEmpty().WithMessage("Point is required.")
            .GreaterThan(0).WithMessage("Point must be greater than zero.");
        RuleFor(x => x.Reason)
            .Length(3,50).WithMessage("Reason length must be between 3 and 50 characters.");
    }
}