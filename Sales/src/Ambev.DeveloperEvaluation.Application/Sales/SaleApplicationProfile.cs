using Ambev.DeveloperEvaluation.Application.Sales.CreateSale;
using Ambev.DeveloperEvaluation.Application.Sales.GetSale;
using Ambev.DeveloperEvaluation.Domain.Entities;
using AutoMapper;

namespace Ambev.DeveloperEvaluation.Application.Sales;

public class SaleApplicationProfile : Profile
{
    public SaleApplicationProfile()
    {
        CreateMap<Sale, CreateSaleResult>();
        CreateMap<Sale, GetSaleResult>();
        CreateMap<SaleItem, SaleItemResult>();
    }
}
