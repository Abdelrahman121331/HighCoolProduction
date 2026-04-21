using ERP.Application.Inventory;
using ERP.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Inventory;

public sealed class StockBalanceService(AppDbContext dbContext) : IStockBalanceService
{
    public async Task<IReadOnlyList<StockBalanceDto>> ListAsync(StockBalanceQuery query, CancellationToken cancellationToken)
    {
        var entriesQuery = dbContext.StockLedgerEntries
            .AsNoTracking()
            .Include(entity => entity.Item)
                .ThenInclude(entity => entity!.BaseUom)
            .Include(entity => entity.Warehouse)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();
            entriesQuery = entriesQuery.Where(entity =>
                entity.Item!.Code.Contains(search) ||
                entity.Item.Name.Contains(search) ||
                entity.Warehouse!.Code.Contains(search) ||
                entity.Warehouse.Name.Contains(search));
        }

        if (query.ItemId.HasValue)
        {
            entriesQuery = entriesQuery.Where(entity => entity.ItemId == query.ItemId.Value);
        }

        if (query.WarehouseId.HasValue)
        {
            entriesQuery = entriesQuery.Where(entity => entity.WarehouseId == query.WarehouseId.Value);
        }

        if (query.TransactionType.HasValue)
        {
            entriesQuery = entriesQuery.Where(entity => entity.TransactionType == query.TransactionType.Value);
        }

        if (query.FromDate.HasValue)
        {
            entriesQuery = entriesQuery.Where(entity => entity.TransactionDate >= query.FromDate.Value);
        }

        if (query.ToDate.HasValue)
        {
            entriesQuery = entriesQuery.Where(entity => entity.TransactionDate <= query.ToDate.Value);
        }

        var entries = await entriesQuery
            .OrderBy(entity => entity.Item!.Code)
            .ThenBy(entity => entity.Warehouse!.Code)
            .ThenBy(entity => entity.TransactionDate)
            .ThenBy(entity => entity.CreatedAt)
            .ThenBy(entity => entity.Id)
            .ToListAsync(cancellationToken);

        return entries
            .GroupBy(entity => new
            {
                entity.ItemId,
                ItemCode = entity.Item!.Code,
                ItemName = entity.Item.Name,
                entity.WarehouseId,
                WarehouseCode = entity.Warehouse!.Code,
                WarehouseName = entity.Warehouse.Name,
                entity.Item.BaseUomId,
                BaseUomCode = entity.Item.BaseUom!.Code,
                BaseUomName = entity.Item.BaseUom.Name
            })
            .Select(group => new StockBalanceDto(
                group.Key.ItemId,
                group.Key.ItemCode,
                group.Key.ItemName,
                group.Key.WarehouseId,
                group.Key.WarehouseCode,
                group.Key.WarehouseName,
                group.Key.BaseUomId,
                group.Key.BaseUomCode,
                group.Key.BaseUomName,
                group.Sum(CalculateSignedBaseQty),
                group.Max(entity => entity.TransactionDate)))
            .OrderBy(entity => entity.ItemCode)
            .ThenBy(entity => entity.WarehouseCode)
            .ToList();
    }

    private static decimal CalculateSignedBaseQty(Domain.Inventory.StockLedgerEntry entry)
    {
        var inQty = entry.QtyIn > 0m ? entry.BaseQty : 0m;
        var outQty = entry.QtyOut > 0m ? entry.BaseQty : 0m;
        return inQty - outQty;
    }
}
