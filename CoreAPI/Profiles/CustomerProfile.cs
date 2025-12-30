using AutoMapper;
using CoreAPI.DTOs.Customers;
using CoreAPI.Models;

namespace CoreAPI.Profiles;

public class CustomerProfile : Profile
{
    public CustomerProfile()
    {
        CreateMap<Customer, CustomerDto>()
            .ConstructUsing(src =>
                new CustomerDto(
                    src.Id,
                    src.Name,
                    src.Email,
                    src.PhoneNumber,
                    src.LoyaltyAccounts.ToList(),
                    src.CreatedAt,
                    src.UpdatedAt));
        // CreateMap<CustomerCreateDto, Customer>()
        //     .ConstructUsing(src =>
        //         new Customer(
        //             Guid.NewGuid().ToString(),
        //             src.Name,
        //             src.Email,
        //             src.PhoneNumber));
        CreateMap<CustomerUpdateDto, Customer>()
            .ForAllMembers(opts
                => opts.Condition((src, dest, srcMember, destMember) => srcMember != null));
    }
}