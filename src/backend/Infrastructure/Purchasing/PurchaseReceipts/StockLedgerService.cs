using ERP.Application.Purchasing.PurchaseReceipts;
using ERP.Domain.Inventory;
using ERP.Domain.Purchasing;
using ERP.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Purchasing.PurchaseReceipts;

public sealed class StockLedgerService(
    AppDbContext dbContext,
    IQuantityConversionService quantityConversionService) : IStockLedgerService
{
    public async Task<IReadOnlyList<StockLedgerEntry>> CreateEntriesAsync(
        PurchaseReceipt receipt,
        string actor,
        CancellationToken cancellationToken)
    {
        var entries = new List<StockLedgerEntry>();
        var runningBalances = new Dictionary<(Guid ItemId, Guid WarehouseId), decimal>();

        foreach (var line in receipt.Lines.OrderBy(entity => entity.LineNo))
        {
            var item = line.Item ?? throw new InvalidOperationException("Purchase receipt posting requires item references.");
            var key = (line.ItemId, receipt.WarehouseId);

            if (!runningBalances.TryGetValue(key, out var runningBalance))
            {
                runningBalance = await dbContext.StockLedgerEntries
                    .Where(entity => entity.ItemId == line.ItemId && entity.WarehouseId == receipt.WarehouseId)
                    .OrderByDescending(entity => entity.TransactionDate)
                    .ThenByDescending(entity => entity.CreatedAt)
                    .Select(entity => entity.RunningBalanceQty)
                    .FirstOrDefaultAsync(cancellationToken);
            }

            var baseQty = await quantityConversionService.ConvertAsync(
                line.ReceivedQty,
                line.UomId,
                item.BaseUomId,
                cancellationToken);

            runningBalance += baseQty;
            runningBalances[key] = runningBalance;

            entries.Add(new StockLedgerEntry
            {
                ItemId = line.ItemId,
                WarehouseId = receipt.WarehouseId,
                TransactionType = StockTransactionType.PurchaseReceipt,
                SourceDocType = SourceDocumentType.PurchaseReceipt,
                SourceDocId = receipt.Id,
                SourceLineId = line.Id,
                QtyIn = line.ReceivedQty,
                QtyOut = 0m,
                UomId = line.UomId,
                BaseQty = baseQty,
                RunningBalanceQty = runningBalance,
                TransactionDate = receipt.ReceiptDate,
                UnitCost = null,
                TotalCost = null,
                CreatedBy = actor
            });
        }

        dbContext.StockLedgerEntries.AddRange(entries);
        return entries;
    }
}
