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
        CreateMap<AccountType, AccountTypeDto>();
        CreateMap<AccountTypeCreateDto, AccountType>();
        CreateMap<Account, AccountProfileDto>()
            .ForMember(dest => dest.AccountType, opt => opt.MapFrom(src => src.AccountType))
            .ForMember(dest => dest.Transactions, opt => opt.MapFrom(src => src.Transactions))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt));
    }

}