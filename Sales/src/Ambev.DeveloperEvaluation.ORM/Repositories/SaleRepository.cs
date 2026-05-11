using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Ambev.DeveloperEvaluation.ORM.Repositories;

public class SaleRepository : ISaleRepository
{
    private readonly DefaultContext _context;

    public SaleRepository(DefaultContext context)
    {
        _context = context;
    }

    public async Task<Sale> CreateAsync(Sale sale, CancellationToken cancellationToken = default)
    {
        if (sale.Id == Guid.Empty)
            sale.Id = Guid.NewGuid();

        foreach (var item in sale.Items)
        {
            item.SaleId = sale.Id;
            if (item.Id == Guid.Empty)
                item.Id = Guid.NewGuid();
        }

        await _context.Sales.AddAsync(sale, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return sale;
    }

    public async Task<Sale?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Sales
            .Include(s => s.Items)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<Sale?> GetBySaleNumberAsync(int saleNumber, CancellationToken cancellationToken = default)
    {
        return await _context.Sales
            .Include(s => s.Items)
            .FirstOrDefaultAsync(s => s.SaleNumber == saleNumber, cancellationToken);
    }

    public async Task<IReadOnlyList<Sale>> ListAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        if (page < 1)
            page = 1;
        if (pageSize < 1)
            pageSize = 20;
        if (pageSize > 100)
            pageSize = 100;

        return await _context.Sales
            .Include(s => s.Items)
            .OrderByDescending(s => s.SaleDate)
            .ThenByDescending(s => s.SaleNumber)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Sales.CountAsync(cancellationToken);
    }

    public async Task<Sale?> UpdateAsync(Sale sale, CancellationToken cancellationToken = default)
    {
        var existing = await _context.Sales
            .Include(s => s.Items)
            .FirstOrDefaultAsync(s => s.Id == sale.Id, cancellationToken);

        if (existing is null)
            return null;

        existing.SaleNumber = sale.SaleNumber;
        existing.SaleDate = sale.SaleDate;
        existing.CustomerId = sale.CustomerId;
        existing.CustomerName = sale.CustomerName;
        existing.BranchId = sale.BranchId;
        existing.BranchName = sale.BranchName;
        existing.TotalAmount = sale.TotalAmount;
        existing.IsCancelled = sale.IsCancelled;

        _context.SaleItems.RemoveRange(existing.Items);
        existing.Items.Clear();

        foreach (var item in sale.Items)
        {
            var newId = item.Id == Guid.Empty ? Guid.NewGuid() : item.Id;
            existing.Items.Add(new SaleItem
            {
                Id = newId,
                SaleId = existing.Id,
                ProductId = item.ProductId,
                ProductDescription = item.ProductDescription,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                DiscountAmount = item.DiscountAmount,
                LineTotal = item.LineTotal,
                IsCancelled = item.IsCancelled
            });
        }

        await _context.SaveChangesAsync(cancellationToken);
        return existing;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var sale = await _context.Sales
            .Include(s => s.Items)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

        if (sale is null)
            return false;

        _context.Sales.Remove(sale);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
