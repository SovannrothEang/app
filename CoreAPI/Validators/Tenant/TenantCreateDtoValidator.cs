using CoreAPI.DTOs.Tenants;
using FluentValidation;

namespace CoreAPI.Validators.Tenant;

public class TenantCreateDtoValidator : AbstractValidator<TenantOnBoardingDto>
{
    public TenantCreateDtoValidator()
    {
        // Tenant validation
        RuleFor(x => x.Tenant.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(100).WithMessage("Name must not exceed 100 characters.");
        
        RuleFor(x => x.Tenant.ExpiryDays)
            .GreaterThan(0).WithMessage("ExpiryDays must be greater than zero.");
        
        RuleFor(x => x.Tenant.PointPerDollar)
            .GreaterThan(0).WithMessage("PointPerDollar must be greater than zero.");
        
        // Owner validation
        RuleFor(x => x.Owner.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Email must be a valid email address.")
            .MaximumLength(256).WithMessage("Email must not exceed 256 characters.");
        
        RuleFor(x => x.Owner.UserName)
            .NotEmpty().WithMessage("UserName is required.")
            .MinimumLength(3).WithMessage("UserName must be at least 3 characters.")
            .MaximumLength(50).WithMessage("UserName must not exceed 50 characters.");
        
        RuleFor(x => x.Owner.FirstName)
            .NotEmpty().WithMessage("FirstName is required.")
            .MaximumLength(100).WithMessage("FirstName must not exceed 100 characters.");
        
        RuleFor(x => x.Owner.LastName)
            .NotEmpty().WithMessage("LastName is required.")
            .MaximumLength(100).WithMessage("LastName must not exceed 100 characters.");
    }
}