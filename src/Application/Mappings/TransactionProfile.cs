using Application.DTOs.Transactions;
using AutoMapper;
using Domain.Entities;

namespace Application.Mappings;

public class TransactionProfile : Profile
{
    public TransactionProfile()
    {
        CreateMap<Transaction, TransactionDto>()
            .ReverseMap();
    }
}