using Ambev.DeveloperEvaluation.Application.Sales;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Domain.Services;
using AutoMapper;
using FluentValidation;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.CreateSale;

public class CreateSaleHandler : IRequestHandler<CreateSaleCommand, CreateSaleResult>
{
    private readonly ISaleRepository _saleRepository;
    private readonly ISaleLineDiscountCalculator _discountCalculator;
    private readonly IMapper _mapper;

    public CreateSaleHandler(
        ISaleRepository saleRepository,
        ISaleLineDiscountCalculator discountCalculator,
        IMapper mapper)
    {
        _saleRepository = saleRepository;
        _discountCalculator = discountCalculator;
        _mapper = mapper;
    }

    public async Task<CreateSaleResult> Handle(CreateSaleCommand command, CancellationToken cancellationToken)
    {
        var validator = new CreateSaleCommandValidator();
        var validationResult = await validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        if (await _saleRepository.GetBySaleNumberAsync(command.SaleNumber, cancellationToken) is not null)
            throw new InvalidOperationException($"Sale number {command.SaleNumber} already exists.");

        var saleId = Guid.NewGuid();
        var (items, total) = SaleLineBuilder.BuildLines(command.Items, saleId, _discountCalculator);

        var sale = new Sale
        {
            Id = saleId,
            SaleNumber = command.SaleNumber,
            SaleDate = command.SaleDate,
            CustomerId = command.CustomerId,
            CustomerName = command.CustomerName,
            BranchId = command.BranchId,
            BranchName = command.BranchName,
            TotalAmount = total,
            IsCancelled = command.IsCancelled,
            Items = items
        };

        var created = await _saleRepository.CreateAsync(sale, cancellationToken);
        return _mapper.Map<CreateSaleResult>(created);
    }
}
