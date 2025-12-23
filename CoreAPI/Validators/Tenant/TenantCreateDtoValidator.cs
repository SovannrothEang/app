using CoreAPI.DTOs.Tenants;
using FluentValidation;

namespace CoreAPI.Validators.Tenant;

public class TenantCreateDtoValidator : AbstractValidator<TenantCreateDto>
{
    public TenantCreateDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(100).WithMessage("Name must not exceed 100 characters.");
        
        RuleFor(x => x.ExpiryDays)
            .NotEmpty().WithMessage("ExpiryDays is required.")
            .GreaterThan(0).WithMessage("ExpiryDays must be greater than zero.");
        
        RuleFor(x => x.PointPerDollar)
            .NotEmpty().WithMessage("PointPerDollar is required.")
            .GreaterThan(0).WithMessage("PointPerDollar must be greater than zero.");
    }
}