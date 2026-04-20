using ERP.Domain.Common;
using ERP.Domain.Purchasing;

namespace ERP.Application.Purchasing.PurchaseOrders;

public sealed record PurchaseOrderDto(
    Guid Id,
    string PoNo,
    Guid SupplierId,
    string SupplierCode,
    string SupplierName,
    DateTime OrderDate,
    DateTime? ExpectedDate,
    string? Notes,
    DocumentStatus Status,
    PurchaseOrderReceiptProgressStatus ReceiptProgressStatus,
    IReadOnlyList<PurchaseOrderLineDto> Lines,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
