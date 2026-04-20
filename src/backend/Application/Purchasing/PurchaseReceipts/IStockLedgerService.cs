using ERP.Domain.Inventory;
using ERP.Domain.Purchasing;

namespace ERP.Application.Purchasing.PurchaseReceipts;

public interface IStockLedgerService
{
    Task<IReadOnlyList<StockLedgerEntry>> CreateEntriesAsync(
        PurchaseReceipt receipt,
        string actor,
        CancellationToken cancellationToken);
}
