using Ambev.DeveloperEvaluation.Application.Sales.GetSale;
using Ambev.DeveloperEvaluation.Domain;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using AutoMapper;
using FluentValidation;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.ListSales;

public class ListSalesHandler : IRequestHandler<ListSalesQuery, ListSalesResult>
{
    private readonly ISaleRepository _saleRepository;
    private readonly IMapper _mapper;

    public ListSalesHandler(ISaleRepository saleRepository, IMapper mapper)
    {
        _saleRepository = saleRepository;
        _mapper = mapper;
    }

    public async Task<ListSalesResult> Handle(ListSalesQuery request, CancellationToken cancellationToken)
    {
        var validator = new ListSalesQueryValidator();
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var pageSize = Math.Clamp(request.PageSize, 1, SalesListPagination.MaxPageSize);
        var total = await _saleRepository.CountAsync(cancellationToken);
        var sales = await _saleRepository.ListAsync(request.Page, pageSize, cancellationToken);

        return new ListSalesResult
        {
            Page = request.Page,
            PageSize = pageSize,
            TotalCount = total,
            Items = _mapper.Map<IList<GetSaleResult>>(sales)
        };
    }
}
