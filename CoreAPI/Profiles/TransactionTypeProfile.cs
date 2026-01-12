using AutoMapper;
using CoreAPI.DTOs.Transactions;
using CoreAPI.Models;

namespace CoreAPI.Profiles;

public class TransactionTypeProfile : Profile
{
    public TransactionTypeProfile()
    {
        CreateMap<TransactionType, TransactionTypeDto>();
        CreateMap<TransactionTypeCreateDto, TransactionType>();
        // Nul forgiven cuz we'll check the authentication and authorization
    }
}