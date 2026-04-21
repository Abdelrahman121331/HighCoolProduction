using ERP.Application.Shortages;
using ERP.Domain.Inventory;
using ERP.Domain.Shortages;
using ERP.Domain.Statements;
using ERP.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Shortages;

public sealed class ShortageResolutionAllocationService(AppDbContext dbContext) : IShortageResolutionAllocationService
{
    public async Task ApplyAsync(ShortageResolution resolution, string actor, CancellationToken cancellationToken)
    {
        var runningStockBalances = new Dictionary<(Guid ItemId, Guid WarehouseId), decimal>();
        var runningSupplierBalances = new Dictionary<Guid, decimal>();

        foreach (var allocation in resolution.Allocations.OrderBy(entity => entity.SequenceNo))
        {
            var shortage = allocation.ShortageLedgerEntry
                ?? throw new InvalidOperationException("Resolution allocation is missing its shortage row.");
            NormalizeLegacyShortageState(shortage);
            var componentItem = shortage.ComponentItem
                ?? throw new InvalidOperationException("Shortage resolution requires component item references.");
            var receipt = shortage.PurchaseReceipt
                ?? throw new InvalidOperationException("Shortage resolution requires receipt traceability.");

            if (resolution.ResolutionType == ShortageResolutionType.Physical)
            {
                var allocatedQty = Round(allocation.AllocatedQty!.Value);
                var rate = EnsureValuationBasis(shortage, allocation.ValuationRate);

                ApplyResolvedState(shortage, allocatedQty, rate, actor);

                var stockKey = (shortage.ComponentItemId, receipt.WarehouseId);
                if (!runningStockBalances.TryGetValue(stockKey, out var stockRunningBalance))
                {
                    stockRunningBalance = await dbContext.StockLedgerEntries
                        .Where(entity => entity.ItemId == shortage.ComponentItemId && entity.WarehouseId == receipt.WarehouseId)
                        .OrderByDescending(entity => entity.TransactionDate)
                        .ThenByDescending(entity => entity.CreatedAt)
                        .Select(entity => entity.RunningBalanceQty)
                        .FirstOrDefaultAsync(cancellationToken);
                }

                stockRunningBalance += allocatedQty;
                runningStockBalances[stockKey] = stockRunningBalance;

                dbContext.StockLedgerEntries.Add(new StockLedgerEntry
                {
                    ItemId = shortage.ComponentItemId,
                    WarehouseId = receipt.WarehouseId,
                    TransactionType = StockTransactionType.ShortagePhysicalResolution,
                    SourceDocType = SourceDocumentType.ShortageResolution,
                    SourceDocId = resolution.Id,
                    SourceLineId = allocation.Id,
                    QtyIn = allocatedQty,
                    QtyOut = 0m,
                    UomId = componentItem.BaseUomId,
                    BaseQty = allocatedQty,
                    RunningBalanceQty = stockRunningBalance,
                    TransactionDate = resolution.ResolutionDate,
                    UnitCost = rate,
                    TotalCost = rate.HasValue ? Round(allocatedQty * rate.Value) : null,
                    CreatedBy = actor
                });

                continue;
            }

            var allocatedAmount = Round(allocation.AllocatedAmount!.Value);
            var valuationRate = EnsureValuationBasis(shortage, allocation.ValuationRate)
                ?? throw new InvalidOperationException("Financial allocations require a valuation basis.");
            var quantityEquivalent = Round(allocatedAmount / valuationRate);

            ApplyResolvedState(shortage, quantityEquivalent, valuationRate, actor);

            if (!runningSupplierBalances.TryGetValue(resolution.SupplierId, out var supplierRunningBalance))
            {
                supplierRunningBalance = await dbContext.SupplierStatementEntries
                    .Where(entity => entity.SupplierId == resolution.SupplierId)
                    .OrderByDescending(entity => entity.TransactionDate)
                    .ThenByDescending(entity => entity.CreatedAt)
                    .Select(entity => entity.RunningBalance)
                    .FirstOrDefaultAsync(cancellationToken);
            }

            supplierRunningBalance -= allocatedAmount;
            runningSupplierBalances[resolution.SupplierId] = supplierRunningBalance;

            dbContext.SupplierStatementEntries.Add(new SupplierStatementEntry
            {
                SupplierId = resolution.SupplierId,
                EffectType = SupplierStatementEffectType.ShortageFinancialResolution,
                SourceDocType = SupplierStatementSourceDocumentType.ShortageResolution,
                SourceDocId = resolution.Id,
                SourceLineId = allocation.Id,
                AmountDelta = -allocatedAmount,
                RunningBalance = supplierRunningBalance,
                Currency = resolution.Currency,
                TransactionDate = resolution.ResolutionDate,
                Notes = $"Shortage resolution {resolution.ResolutionNo}",
                CreatedBy = actor
            });
        }
    }

    private static decimal? EnsureValuationBasis(ShortageLedgerEntry shortage, decimal? providedRate)
    {
        var existingRate = shortage.ShortageValue.HasValue && shortage.ShortageQty > 0m
            ? Round(shortage.ShortageValue.Value / shortage.ShortageQty)
            : (decimal?)null;

        if (existingRate.HasValue)
        {
            if (providedRate.HasValue && Round(providedRate.Value) != existingRate.Value)
            {
                throw new InvalidOperationException("Allocation valuation rate must match the shortage row valuation basis.");
            }

            return existingRate;
        }

        if (!providedRate.HasValue)
        {
            return null;
        }

        var rate = Round(providedRate.Value);
        shortage.ShortageValue = Round(shortage.ShortageQty * rate);
        return rate;
    }

    private static void ApplyResolvedState(ShortageLedgerEntry shortage, decimal quantityEquivalent, decimal? rate, string actor)
    {
        shortage.ResolvedQty = Round(shortage.ResolvedQty + quantityEquivalent);
        shortage.OpenQty = ClampToZero(Round(shortage.ShortageQty - shortage.ResolvedQty));

        if (rate.HasValue)
        {
            shortage.ShortageValue ??= Round(shortage.ShortageQty * rate.Value);
            shortage.ResolvedAmount = Round(shortage.ResolvedQty * rate.Value);
            shortage.OpenAmount = ClampNullableToZero(Round(shortage.ShortageValue.Value - shortage.ResolvedAmount));
        }

        shortage.Status = shortage.OpenQty == 0m
            ? ShortageEntryStatus.Resolved
            : shortage.ResolvedQty > 0m || shortage.ResolvedAmount > 0m
                ? ShortageEntryStatus.PartiallyResolved
                : ShortageEntryStatus.Open;
        shortage.UpdatedBy = actor;
    }

    private static decimal ClampToZero(decimal value)
    {
        return Math.Abs(value) < 0.000001m ? 0m : value;
    }

    private static decimal? ClampNullableToZero(decimal value)
    {
        return Math.Abs(value) < 0.000001m ? 0m : value;
    }

    private static decimal Round(decimal value)
    {
        return decimal.Round(value, 6, MidpointRounding.AwayFromZero);
    }

    private static void NormalizeLegacyShortageState(ShortageLedgerEntry shortage)
    {
        if (shortage.Status is ShortageEntryStatus.Resolved or ShortageEntryStatus.Canceled)
        {
            return;
        }

        if (shortage.OpenQty <= 0m && shortage.ShortageQty > shortage.ResolvedQty)
        {
            shortage.OpenQty = Round(shortage.ShortageQty - shortage.ResolvedQty);
        }

        if (!shortage.OpenAmount.HasValue && shortage.ShortageValue.HasValue)
        {
            shortage.OpenAmount = Round(shortage.ShortageValue.Value - shortage.ResolvedAmount);
        }
    }
}
