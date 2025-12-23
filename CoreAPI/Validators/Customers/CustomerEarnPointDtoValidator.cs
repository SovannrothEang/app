using CoreAPI.DTOs.Customers;
using FluentValidation;

namespace CoreAPI.Validators.Customers;

public class CustomerEarnPointDtoValidator : AbstractValidator<CustomerEarnPointDto>
{
    public CustomerEarnPointDtoValidator()
    {
        RuleFor(x => x.Amount)
            .NotEmpty().WithMessage("Point is required.")
            .GreaterThan(0).WithMessage("Point must be greater than 0.");
        RuleFor(x => x.Reason)
            .Length(3, 50).WithMessage("Reason length must be between 3 and 50 characters.");
    }
}