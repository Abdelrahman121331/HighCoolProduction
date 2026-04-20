namespace ERP.Application.Purchasing.PurchaseReceipts;

public interface IPurchaseReceiptPostingService
{
    Task<PurchaseReceiptDto?> PostAsync(Guid id, string actor, CancellationToken cancellationToken);
}
