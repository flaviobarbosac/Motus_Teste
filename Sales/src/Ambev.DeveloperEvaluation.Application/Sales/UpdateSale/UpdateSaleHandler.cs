using System.Linq;
using Ambev.DeveloperEvaluation.Application.Sales;
using Ambev.DeveloperEvaluation.Application.Sales.GetSale;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Events;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Domain.Services;
using AutoMapper;
using FluentValidation;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.UpdateSale;

public class UpdateSaleHandler : IRequestHandler<UpdateSaleCommand, GetSaleResult>
{
    private readonly ISaleRepository _saleRepository;
    private readonly ISaleLineDiscountCalculator _discountCalculator;
    private readonly ISaleLifecycleEvents _saleLifecycleEvents;
    private readonly IMapper _mapper;

    public UpdateSaleHandler(
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

    public async Task<GetSaleResult> Handle(UpdateSaleCommand command, CancellationToken cancellationToken)
    {
        var validator = new UpdateSaleCommandValidator();
        var validationResult = await validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var existing = await _saleRepository.GetByIdAsync(command.Id, cancellationToken);
        if (existing is null)
            throw new KeyNotFoundException($"Sale with ID {command.Id} not found");

        var duplicate = await _saleRepository.GetBySaleNumberAsync(command.SaleNumber, cancellationToken);
        if (duplicate is not null && duplicate.Id != command.Id)
            throw new InvalidOperationException($"Sale number {command.SaleNumber} already exists.");

        var (items, total) = SaleLineBuilder.BuildLines(command.Items, command.Id, _discountCalculator);

        var sale = new Sale
        {
            Id = command.Id,
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

        var updated = await _saleRepository.UpdateAsync(sale, cancellationToken);
        if (updated is null)
            throw new KeyNotFoundException($"Sale with ID {command.Id} not found");

        var now = DateTime.UtcNow;
        _saleLifecycleEvents.SaleModified(updated.Id, updated.SaleNumber, updated.TotalAmount, now);
        if (command.IsCancelled && !existing.IsCancelled)
            _saleLifecycleEvents.SaleCancelled(updated.Id, updated.SaleNumber, "SaleMarkedCancelled", now);
        foreach (var line in command.Items.Where(i => i.IsCancelled))
            _saleLifecycleEvents.ItemCancelled(updated.Id, line.ProductId, line.ProductDescription, line.Quantity, now);

        return _mapper.Map<GetSaleResult>(updated);
    }
}
