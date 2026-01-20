using CoreAPI.DTOs.Customers;
using CoreAPI.Models;
using CoreAPI.Repositories.Interfaces;
using FluentValidation;

namespace CoreAPI.Validators.Customers;

public class PostTransactionValidator : AbstractValidator<PostTransactionDto>
{
    public PostTransactionValidator(IUnitOfWork unitOfWork)
    {
        var repo = unitOfWork.GetRepository<Customer>();
        RuleFor(x => x.Amount)
            .NotEmpty().WithMessage("Point is required.")
            .NotEqual(0).WithMessage("Point must not be zero.");
        RuleFor(x => x.Reason)
            .Length(3, 50).WithMessage("Reason length must be between 3 and 50 characters.");
        RuleFor(x => x.ReferenceId)
            .MaximumLength(100).WithMessage("Reference id must not exceed 100 characters.")
            .MustAsync(async (id, cancellation) =>
            {
                if (id is null) return true;
                return await repo.ExistsAsync(
                    predicate: c => c.Id == id,
                    cancellationToken: cancellation);
            })
            .WithMessage("The specified reference does not exist.");
    }
}