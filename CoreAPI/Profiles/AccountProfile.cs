using System.Configuration;
using AutoMapper;
using CoreAPI.DTOs.Accounts;
using CoreAPI.Models;

namespace CoreAPI.Profiles;

public class AccountProfile : Profile
{
    public AccountProfile()
    {
        CreateMap<Account, AccountDto>()
            .ReverseMap();
        // .ConstructUsing(src => new AccountDto(
        //     src.TenantId,
        //     src.CustomerId,
        //     src.Balance,
        //     src.Transactions.ToList(),
        //     src.PerformBy,
        //     src.PerformByUser));
    }

}