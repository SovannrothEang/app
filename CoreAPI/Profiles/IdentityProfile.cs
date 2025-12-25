using AutoMapper;
using CoreAPI.DTOs;
using CoreAPI.DTOs.Auth;
using CoreAPI.DTOs.Tenants;
using CoreAPI.Models;

namespace CoreAPI.Profiles;

public class IdentityProfile : Profile
{
    public IdentityProfile()
    {
        // User
        CreateMap<LoginDto, User>();
        CreateMap<RegisterDto, User>();
        CreateMap<TenantOwnerCreate, User>();
        CreateMap<User, UserProfileResponseDto>()
            .ConstructUsing(src => new UserProfileResponseDto(
                src.Id,
                src.UserName,
                src.Email,
                new List<string>()
            ))
            .ReverseMap();
        // Role
        CreateMap<RoleCreateDto, Role>();
        CreateMap<Role, RoleDto>()
            .ReverseMap();
    }
}
