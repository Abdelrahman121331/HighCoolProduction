using ERP.Domain.Purchasing;
using ERP.Domain.Shortages;

namespace ERP.Application.Purchasing.PurchaseReceipts;

public interface IShortageDetectionService
{
    Task<IReadOnlyList<ShortageLedgerEntry>> CreateEntriesAsync(
        PurchaseReceipt receipt,
        string actor,
        CancellationToken cancellationToken);
}
