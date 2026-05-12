using Ambev.DeveloperEvaluation.Application.Sales.CreateSale;
using Ambev.DeveloperEvaluation.Application.Sales.GetSale;
using Ambev.DeveloperEvaluation.Application.Sales.UpdateSale;
using Ambev.DeveloperEvaluation.WebApi.Features.Sales.CreateSale;
using Ambev.DeveloperEvaluation.WebApi.Features.Sales.GetSale;
using AutoMapper;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Sales;

public class SalesWebApiProfile : Profile
{
    public SalesWebApiProfile()
    {
        CreateMap<CreateSaleRequest, CreateSaleCommand>();
        CreateMap<CreateSaleItemRequest, CreateSaleItemCommand>();
        CreateMap<CreateSaleRequest, UpdateSaleCommand>()
            .ForMember(d => d.Id, o => o.Ignore());

        CreateMap<CreateSaleResult, CreateSaleResponse>();
        CreateMap<GetSaleResult, GetSaleResponse>();
        CreateMap<SaleItemResult, SaleItemResponse>();
    }
}
