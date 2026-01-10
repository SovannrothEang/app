using System.ComponentModel.DataAnnotations;

namespace CoreAPI.DTOs;

public class PaginationOption
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;

    // TODO: make sorting and filtering work
    private string _sort = string.Empty;
    private static readonly string[] SortOption = ["asc", "desc"];
    // public string SortDirection
    // {
    //     get  => _sort;
    //     set 
    //     {
    //         if (!SortOption.Contains(value))
    //             throw new InvalidOperationException("Only 'asc'  and 'desc' are supported.");
    //         _sort = value;
    //     }
    // }
}