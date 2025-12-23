namespace CoreAPI.DTOs.Customers;

public record Pagination(
    int? Page = 1,
    int? PageSize = int.MaxValue);