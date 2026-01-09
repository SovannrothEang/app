using AutoMapper;
using CoreAPI.DTOs.Customers;
using CoreAPI.Models;

namespace CoreAPI.Profiles;

public class CustomerProfile : Profile
{
    public CustomerProfile()
    {
        CreateMap<Customer, CustomerDto>()
            .ForMember(dest => dest.UserName, opt
                => opt.MapFrom(src => src.User!.UserName))
            .ForMember(dest => dest.Email, opt
                => opt.MapFrom(src => src.User!.Email))
            .ForMember(dest => dest.PhoneNumber, opt
                => opt.MapFrom(src => src.User!.PhoneNumber))
            .ForMember(dest => dest.Accounts, opt
                => opt.MapFrom(src => src.Accounts.ToList()))
            .ReverseMap();
        // .ConstructUsing(src =>
        //     new CustomerDto(
        //         src.Id,
        //         src.User!.UserName,
        //         src.User.Email,
        //         src.User.PhoneNumber,
        //         src.Accounts.ToList(),
        //         src.CreatedAt,
        //         src.UpdatedAt));
        // CreateMap<CustomerCreateDto, Customer>()
        //     .ConstructUsing(src =>
        //         new Customer(
        //             Guid.NewGuid().ToString(),
        //             src.Name,
        //             src.Email,
        //             src.PhoneNumber));
        CreateMap<CustomerUpdateDto, Customer>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember, destMember) => srcMember != null));
    }
}