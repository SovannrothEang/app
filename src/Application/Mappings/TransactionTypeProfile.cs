using Application.DTOs.Transactions;
using AutoMapper;
using Domain.Entities;

namespace Application.Mappings;

public class TransactionTypeProfile : Profile
{
    public TransactionTypeProfile()
    {
        CreateMap<TransactionType, TransactionTypeDto>();
        CreateMap<TransactionTypeCreateDto, TransactionType>();
        // Nul forgiven cuz we'll check the authentication and authorization
    }
}