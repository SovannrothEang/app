namespace CoreAPI.DTOs;

public class PaginationOption
//     int? Page = 1,
//     int? PageSize = 10,
//     string? SortBy = "CreatedAt",
//     string? SortDirection = "asc",
//     string? TransactionType = null,
//     DateOnly? StartDate = null,
//     DateOnly? EndDate = null
// )
{
    public int? Page { get; set; } = 1;
    public int? PageSize { get; set; } = 10;
    public string? SortBy { get; set; } = "CreatedAt";
    public string? SortDirection { get; set; } = "asc";
    public string? FilterBy { get; set; } = null;
    public string? FilterValue { get; set; } = null;
    public DateOnly? StartDate { get; set; } = null;
    public DateOnly? EndDate { get; set; } = null;
}