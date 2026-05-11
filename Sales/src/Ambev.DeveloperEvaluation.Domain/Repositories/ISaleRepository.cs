using Ambev.DeveloperEvaluation.Domain.Entities;

namespace Ambev.DeveloperEvaluation.Domain.Repositories;

/// <summary>
/// Persistence for the Sale aggregate (header + line items).
/// </summary>
public interface ISaleRepository
{
    Task<Sale> CreateAsync(Sale sale, CancellationToken cancellationToken = default);

    Task<Sale?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Sale?> UpdateAsync(Sale sale, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
