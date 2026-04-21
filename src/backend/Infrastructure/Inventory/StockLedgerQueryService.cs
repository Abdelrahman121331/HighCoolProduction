using ERP.Application.Inventory;
using ERP.Domain.Inventory;
using ERP.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Inventory;

public sealed class StockLedgerQueryService(AppDbContext dbContext) : IStockLedgerQueryService
{
    public async Task<IReadOnlyList<StockLedgerEntryDto>> ListAsync(StockLedgerQuery query, CancellationToken cancellationToken)
    {
        var entriesQuery = dbContext.StockLedgerEntries
            .AsNoTracking()
            .Include(entity => entity.Item)
            .Include(entity => entity.Warehouse)
            .Include(entity => entity.Uom)
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
            .OrderByDescending(entity => entity.TransactionDate)
            .ThenByDescending(entity => entity.CreatedAt)
            .ThenByDescending(entity => entity.Id)
            .ToListAsync(cancellationToken);

        var purchaseReceiptIds = entries
            .Where(entity => entity.SourceDocType == SourceDocumentType.PurchaseReceipt)
            .Select(entity => entity.SourceDocId)
            .Distinct()
            .ToArray();

        var shortageResolutionIds = entries
            .Where(entity => entity.SourceDocType == SourceDocumentType.ShortageResolution)
            .Select(entity => entity.SourceDocId)
            .Distinct()
            .ToArray();

        var purchaseReceiptNumbers = purchaseReceiptIds.Length == 0
            ? new Dictionary<Guid, string>()
            : await dbContext.PurchaseReceipts
                .AsNoTracking()
                .Where(entity => purchaseReceiptIds.Contains(entity.Id))
                .ToDictionaryAsync(entity => entity.Id, entity => entity.ReceiptNo, cancellationToken);

        var shortageResolutionNumbers = shortageResolutionIds.Length == 0
            ? new Dictionary<Guid, string>()
            : await dbContext.ShortageResolutions
                .AsNoTracking()
                .Where(entity => shortageResolutionIds.Contains(entity.Id))
                .ToDictionaryAsync(entity => entity.Id, entity => entity.ResolutionNo, cancellationToken);

        return entries
            .Select(entity => new StockLedgerEntryDto(
                entity.Id,
                entity.ItemId,
                entity.Item!.Code,
                entity.Item.Name,
                entity.WarehouseId,
                entity.Warehouse!.Code,
                entity.Warehouse.Name,
                entity.TransactionType,
                entity.SourceDocType,
                entity.SourceDocId,
                entity.SourceLineId,
                ResolveSourceDocumentNo(entity, purchaseReceiptNumbers, shortageResolutionNumbers),
                entity.QtyIn,
                entity.QtyOut,
                entity.UomId,
                entity.Uom!.Code,
                entity.Uom.Name,
                entity.BaseQty,
                entity.RunningBalanceQty,
                entity.TransactionDate,
                entity.UnitCost,
                entity.TotalCost,
                entity.CreatedAt,
                entity.CreatedBy))
            .ToList();
    }

    private static string ResolveSourceDocumentNo(
        StockLedgerEntry entity,
        IReadOnlyDictionary<Guid, string> purchaseReceiptNumbers,
        IReadOnlyDictionary<Guid, string> shortageResolutionNumbers)
    {
        return entity.SourceDocType switch
        {
            SourceDocumentType.PurchaseReceipt when purchaseReceiptNumbers.TryGetValue(entity.SourceDocId, out var receiptNo) => receiptNo,
            SourceDocumentType.ShortageResolution when shortageResolutionNumbers.TryGetValue(entity.SourceDocId, out var resolutionNo) => resolutionNo,
            _ => entity.SourceDocId.ToString()
        };
    }
}
