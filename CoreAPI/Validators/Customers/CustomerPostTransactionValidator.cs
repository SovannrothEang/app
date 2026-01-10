using CoreAPI.DTOs.Customers;
using CoreAPI.Repositories.Interfaces;
using CoreAPI.Services.Interfaces;
using FluentValidation;

namespace CoreAPI.Validators.Customers;

public class CustomerPostTransactionValidator : AbstractValidator<CustomerPostTransaction>
{
    public CustomerPostTransactionValidator(ICustomerRepository repository)
    {
        var repo = repository;
        RuleFor(x => x.Amount)
            .NotEmpty().WithMessage("Point is required.")
            .GreaterThan(0).WithMessage("Point must be greater than 0.");
        RuleFor(x => x.Reason)
            .Length(3, 50).WithMessage("Reason length must be between 3 and 50 characters.");
        RuleFor(x => x.ReferenceId)
            .MaximumLength(100).WithMessage("Reference id must not exceed 100 characters.")
            .MustAsync(async (id, cancellation) =>
            {
                if (id is null) return true;
                return await repo.ExistsInTenantAsync(id, cancellation);
            })
            .WithMessage("The specified reference does not exist.");
    }
}