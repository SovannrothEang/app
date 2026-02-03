using Application.DTOs.Auth;
using Application.Services;
using Domain.Entities;
using FluentValidation;
using Microsoft.AspNetCore.Identity;

namespace Application.Validators.Auth;

public class ChangePasswordRequestValidator : AbstractValidator<ChangePasswordRequest>
{
    private readonly UserManager<User> _userManager;
    private readonly ICurrentUserProvider  _currentUserProvider;

    public ChangePasswordRequestValidator(UserManager<User> userManager,
        ICurrentUserProvider currentUserProvider)
    {
        _userManager = userManager;
        _currentUserProvider = currentUserProvider;

        RuleFor(e => e.CurrentPassword)
            .NotEmpty().WithMessage("Current password is required.")
            .NotNull().WithMessage("Current password is required.")
            .MustAsync(async (currentPwd, cancellation) => await CheckCurrentPassword(currentPwd, cancellation));

        RuleFor(e => e.NewPassword)
            .NotEmpty().WithMessage("New password is required.")
            .NotNull().WithMessage("New password is required.");

        RuleFor(e => e.ConfirmPassword)
            .NotEmpty().WithMessage("Confirm password is required.")
            .NotNull().WithMessage("Confirm password is required.")
            .Equal(x => x.NewPassword).WithMessage("Confirm password must match the new password!");
    }

    private async Task<bool> CheckCurrentPassword(string currentPassword, CancellationToken ct = default)
    {
        var userId = _currentUserProvider.UserId ?? throw new UnauthorizedAccessException();
        var user = await _userManager.FindByIdAsync(userId);
        return user is null
            ? throw new KeyNotFoundException($"No user was found with id: {userId}.")
            : await _userManager.CheckPasswordAsync(user, currentPassword);
    }
}