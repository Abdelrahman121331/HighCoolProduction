namespace ERP.Application.Purchasing.PurchaseReceipts;

public interface IQuantityConversionService
{
    Task<decimal> ConvertAsync(decimal quantity, Guid fromUomId, Guid toUomId, CancellationToken cancellationToken);
}
