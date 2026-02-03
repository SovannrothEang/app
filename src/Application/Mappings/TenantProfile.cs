using Application.DTOs.Tenants;
using AutoMapper;
using Domain.Entities;

namespace Application.Mappings;

public class TenantProfile : Profile
{
    public TenantProfile()
    {
        // CreateMap<TenantCreateDto, Tenant>()
        //     .ForMember(dest => dest.Name, opt =>
        //         opt.MapFrom(src => TenantName.Create(src.Name)))
        //     .ForMember(dest => dest.Setting.PointExpirePeriod, opt =>
        //         opt.MapFrom(src => src.Setting.PointExpirePeriod))
        CreateMap<TenantCreateDto, Tenant>()
            .ForMember(dest => dest.Slug, opt => opt.MapFrom(src => src.Name.ToLower().Replace(" ", "-")))
            .ForMember(dest => dest.Setting, opt => opt.MapFrom(src =>
                src.PointPerDollar != null && src.ExpiryDays != null ? new AccountSetting(src.PointPerDollar.Value, src.ExpiryDays.Value) : null));

        // CreateMap<TenantUpdateDto, Tenant>()
        //     .ForMember(dest => dest.Name, opt
        //         => opt.Condition(src => src.Name != null))
        //     .ForMember(dest => dest.Setting.ExpiryDays, opt
        //         => opt.Condition(src => src.ExpiryDays != null))
        //     .ForMember(dest => dest.Setting.PointPerDollar, opt
        //         => opt.Condition(src => src.PointPerDollar != null));
            // .ForAllMembers(opts
            //     => opts.Condition((src, dest, srcMember) => srcMember != null));
        
        CreateMap<Tenant, TenantDto>()
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.Setting, opt => opt.MapFrom(src => src.Setting))
            .ReverseMap();
    }
}

// {
//     public LoyaltyProgramSetting Resolve(TenantUpdateDto source, Tenant destination, LoyaltyProgramSetting destMember,
//         ResolutionContext context)
//     {
//         if (source.ExpiryDays != null)
//         {
//             // destination.UpdateSetting(new LoyaltyProgramSetting) = source.ExpiryDays;
//         }
//     }
// }