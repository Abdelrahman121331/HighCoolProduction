using ERP.Application.Shortages;
using ERP.Domain.Shortages;
using ERP.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Shortages;

public sealed class ShortageResolutionValidationService(AppDbContext dbContext) : IShortageResolutionValidationService
{
    public async Task ValidateDraftAsync(ShortageResolution resolution, CancellationToken cancellationToken)
    {
        if (resolution.Status != Domain.Common.DocumentStatus.Draft)
        {
            throw new InvalidOperationException("Only Draft shortage resolutions can be posted.");
        }

        var supplier = await dbContext.Suppliers
            .AsNoTracking()
            .SingleOrDefaultAsync(entity => entity.Id == resolution.SupplierId, cancellationToken);

        if (supplier is null || !supplier.IsActive)
        {
            throw new InvalidOperationException("Supplier was not found.");
        }

        if (resolution.Allocations.Count == 0)
        {
            throw new InvalidOperationException("At least one allocation is required before posting.");
        }

        var duplicateShortageIds = resolution.Allocations
            .GroupBy(entity => entity.ShortageLedgerId)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToArray();

        if (duplicateShortageIds.Length > 0)
        {
            throw new InvalidOperationException("Duplicate shortage allocations are not allowed inside the same resolution.");
        }

        foreach (var allocation in resolution.Allocations.OrderBy(entity => entity.SequenceNo))
        {
            var shortage = allocation.ShortageLedgerEntry
                ?? throw new InvalidOperationException("Each allocation must reference a valid shortage row.");
            NormalizeLegacyShortageState(shortage);

            if (shortage.PurchaseReceipt?.SupplierId != resolution.SupplierId)
            {
                throw new InvalidOperationException("Every allocation must belong to the selected supplier.");
            }

            if (shortage.Status is ShortageEntryStatus.Canceled or ShortageEntryStatus.Resolved || shortage.OpenQty <= 0m)
            {
                throw new InvalidOperationException("Resolved or canceled shortage rows cannot be allocated again.");
            }

            if (resolution.ResolutionType == ShortageResolutionType.Physical)
            {
                if (!allocation.AllocatedQty.HasValue || allocation.AllocatedQty.Value <= 0m)
                {
                    throw new InvalidOperationException("Physical resolutions require allocated quantities.");
                }

                if (allocation.AllocatedAmount.HasValue)
                {
                    throw new InvalidOperationException("Physical resolutions cannot include allocated amounts.");
                }

                if (allocation.AllocatedQty.Value > shortage.OpenQty)
                {
                    throw new InvalidOperationException("Allocated quantity cannot exceed the open shortage quantity.");
                }
            }
            else
            {
                if (!shortage.AffectsSupplierBalance)
                {
                    throw new InvalidOperationException("Financial resolution is allowed only for shortage rows that affect supplier balance.");
                }

                if (!allocation.AllocatedAmount.HasValue || allocation.AllocatedAmount.Value <= 0m)
                {
                    throw new InvalidOperationException("Financial resolutions require allocated amounts.");
                }

                if (allocation.AllocatedQty.HasValue)
                {
                    throw new InvalidOperationException("Financial resolutions cannot include allocated quantities.");
                }

                var rate = ResolveRate(shortage, allocation.ValuationRate);
                if (!rate.HasValue || rate.Value <= 0m)
                {
                    throw new InvalidOperationException("Financial allocations require a valuation rate when the shortage row does not yet have one.");
                }

                var openAmount = shortage.ShortageValue.HasValue
                    ? shortage.OpenAmount ?? Round(shortage.OpenQty * rate.Value)
                    : Round(shortage.OpenQty * rate.Value);

                if (allocation.AllocatedAmount.Value > openAmount)
                {
                    throw new InvalidOperationException("Allocated amount cannot exceed the open shortage amount.");
                }

                var quantityEquivalent = Round(allocation.AllocatedAmount.Value / rate.Value);
                if (quantityEquivalent > shortage.OpenQty)
                {
                    throw new InvalidOperationException("Allocated amount cannot resolve more than the open shortage quantity.");
                }
            }
        }
    }

    private static decimal? ResolveRate(ShortageLedgerEntry shortage, decimal? valuationRate)
    {
        if (shortage.ShortageValue.HasValue && shortage.ShortageQty > 0m)
        {
            var existingRate = Round(shortage.ShortageValue.Value / shortage.ShortageQty);

            if (valuationRate.HasValue && Round(valuationRate.Value) != existingRate)
            {
                throw new InvalidOperationException("Allocation valuation rate must match the shortage row valuation basis.");
            }

            return existingRate;
        }

        return valuationRate.HasValue ? Round(valuationRate.Value) : null;
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
