using AutoMapper;
using CoreAPI.DTOs.Transactions;
using CoreAPI.Models;

namespace CoreAPI.Profiles;

public class TransactionTypeProfile : Profile
{
    public TransactionTypeProfile()
    {
        CreateMap<TransactionType, TransactionTypeDto>();
        CreateMap<TransactionTypeCreateDto, TransactionType>()
            .ConstructUsing(src => new TransactionType(
                Guid.NewGuid().ToString(),
                src.Slug,
                src.Name,
                src.Description,
                src.Multiplier,
                src.AllowNegative));
        // Nul forgiven cuz we'll check the authentication and authorization
    }
}