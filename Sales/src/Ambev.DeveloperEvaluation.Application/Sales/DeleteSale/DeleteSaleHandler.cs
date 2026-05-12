using Ambev.DeveloperEvaluation.Domain.Events;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using FluentValidation;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.DeleteSale;

public class DeleteSaleHandler : IRequestHandler<DeleteSaleCommand, DeleteSaleResponse>
{
    private readonly ISaleRepository _saleRepository;
    private readonly ISaleLifecycleEvents _saleLifecycleEvents;

    public DeleteSaleHandler(ISaleRepository saleRepository, ISaleLifecycleEvents saleLifecycleEvents)
    {
        _saleRepository = saleRepository;
        _saleLifecycleEvents = saleLifecycleEvents;
    }

    public async Task<DeleteSaleResponse> Handle(DeleteSaleCommand request, CancellationToken cancellationToken)
    {
        var validator = new DeleteSaleValidator();
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var sale = await _saleRepository.GetByIdAsync(request.Id, cancellationToken);
        if (sale is null)
            throw new KeyNotFoundException($"Sale with ID {request.Id} not found");

        var success = await _saleRepository.DeleteAsync(request.Id, cancellationToken);
        if (!success)
            throw new KeyNotFoundException($"Sale with ID {request.Id} not found");

        _saleLifecycleEvents.SaleCancelled(sale.Id, sale.SaleNumber, "SaleDeleted", DateTime.UtcNow);

        return new DeleteSaleResponse { Success = true };
    }
}
