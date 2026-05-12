using System.Linq;
using Ambev.DeveloperEvaluation.Application.Sales;
using Ambev.DeveloperEvaluation.Domain.Events;
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
    private readonly ISaleLifecycleEvents _saleLifecycleEvents;
    private readonly IMapper _mapper;

    public CreateSaleHandler(
        ISaleRepository saleRepository,
        ISaleLineDiscountCalculator discountCalculator,
        ISaleLifecycleEvents saleLifecycleEvents,
        IMapper mapper)
    {
        _saleRepository = saleRepository;
        _discountCalculator = discountCalculator;
        _saleLifecycleEvents = saleLifecycleEvents;
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
        var now = DateTime.UtcNow;
        _saleLifecycleEvents.SaleCreated(created.Id, created.SaleNumber, created.TotalAmount, now);
        foreach (var line in command.Items.Where(i => i.IsCancelled))
            _saleLifecycleEvents.ItemCancelled(created.Id, line.ProductId, line.ProductDescription, line.Quantity, now);
        if (command.IsCancelled)
            _saleLifecycleEvents.SaleCancelled(created.Id, created.SaleNumber, "SaleCreatedAsCancelled", now);

        return _mapper.Map<CreateSaleResult>(created);
    }
}
