namespace ERP.Application.Purchasing.PurchaseOrders;

public interface IPurchaseOrderPostingService
{
    Task<PurchaseOrderDto?> PostAsync(Guid id, string actor, CancellationToken cancellationToken);
}
