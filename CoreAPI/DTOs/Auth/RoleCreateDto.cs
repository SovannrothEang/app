using System.ComponentModel.DataAnnotations;

namespace CoreAPI.DTOs.Auth;

public record RoleCreateDto([StringLength(100)] string Name);
