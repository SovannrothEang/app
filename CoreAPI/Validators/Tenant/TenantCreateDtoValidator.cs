using CoreAPI.DTOs.Tenants;
using FluentValidation;

namespace CoreAPI.Validators.Tenant;

public class TenantCreateDtoValidator : AbstractValidator<TenantOnBoardingDto>
{
    public TenantCreateDtoValidator()
    {
        RuleFor(x => x.Tenant.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(100).WithMessage("Name must not exceed 100 characters.");
        
        RuleFor(x => x.Tenant.ExpiryDays)
            .NotEmpty().WithMessage("ExpiryDays is required.")
            .GreaterThan(0).WithMessage("ExpiryDays must be greater than zero.");
        
        RuleFor(x => x.Tenant.PointPerDollar)
            .NotEmpty().WithMessage("PointPerDollar is required.")
            .GreaterThan(0).WithMessage("PointPerDollar must be greater than zero.");
    }
}