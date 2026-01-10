using AutoMapper;
using CoreAPI.DTOs.Transactions;
using CoreAPI.Models;

namespace CoreAPI.Profiles;

public class TransactionProfile : Profile
{
    public TransactionProfile()
    {
        CreateMap<Transaction, TransactionDto>()
            .ReverseMap();
    }
}