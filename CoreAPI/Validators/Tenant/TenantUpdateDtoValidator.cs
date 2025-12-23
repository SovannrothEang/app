using CoreAPI.DTOs.Tenants;
using FluentValidation;

namespace CoreAPI.Validators.Tenant;

public class TenantUpdateDtoValidator : AbstractValidator<TenantUpdateDto>
{
    public TenantUpdateDtoValidator()
    {
        
        RuleFor(x => x.Name)
            .MaximumLength(100).WithMessage("Name must not exceed 100 characters.");
        
        RuleFor(x => x.ExpiryDays)
            .GreaterThan(0).WithMessage("ExpiryDays must be greater than zero.");
        
        RuleFor(x => x.PointPerDollar)
            .GreaterThan(0).WithMessage("PointPerDollar must be greater than zero.");
    }
}