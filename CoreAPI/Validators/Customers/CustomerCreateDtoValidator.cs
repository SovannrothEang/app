using CoreAPI.DTOs.Customers;
using FluentValidation;

namespace CoreAPI.Validators.Customers;

public class CustomerCreateDtoValidator : AbstractValidator<CustomerCreateDto>
{
    public CustomerCreateDtoValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Email is required.")
            .MaximumLength(100).WithMessage("Maximum length of Email is 100");
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(100).WithMessage("Maximum length of Name is 100");
        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("Phone number is required.")
            .Length(8, 20).WithMessage("Phone number must be between 8 and 20 characters long.");
    }
}