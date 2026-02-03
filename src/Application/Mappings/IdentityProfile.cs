using Application.DTOs;
using Application.DTOs.Auth;
using Application.DTOs.Tenants;
using AutoMapper;
using Domain.Entities;

namespace Application.Mappings;

public class IdentityProfile : Profile
{
    public IdentityProfile()
    {
        // User
        CreateMap<LoginDto, User>();
        CreateMap<OnboardingUserDto, User>().ReverseMap();
        CreateMap<RegisterDto, User>();
        CreateMap<TenantOwnerCreateDto, User>();
        CreateMap<User, UserProfileDto>()
            .ConstructUsing(src => new UserProfileDto(
                src.Id,
                src.UserName,
                src.Email,
                src.UserRoles!.Where(x => x.Role != null && x.Role.Name != null).Select(x => x.Role!.Name!).ToList()
            ))
            .ReverseMap();
        // Role
        CreateMap<RoleCreateDto, Role>();
        CreateMap<Role, RoleDto>()
            .ConstructUsing(src => new RoleDto(
                src.Id,
                src.Name ?? string.Empty,
                src.CreatedAt,
                src.UpdatedAt))
            .ReverseMap();
    }
}
