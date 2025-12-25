using CoreAPI.DTOs.LoyaltyAccounts;

namespace CoreAPI.DTOs.Customers;

public record CustomerDetailDto(
    string Id,
    string Name,
    string Email,
    string PhoneNumber,
    int TotalBalance,
    IReadOnlyList<LoyaltyAccountCustomerDto> LoyaltyAccounts);