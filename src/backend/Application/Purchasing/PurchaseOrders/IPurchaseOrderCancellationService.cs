namespace ERP.Application.Purchasing.PurchaseOrders;

public interface IPurchaseOrderCancellationService
{
    Task<PurchaseOrderDto?> CancelAsync(Guid id, string actor, CancellationToken cancellationToken);
}
