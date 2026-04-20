using ERP.Domain.Common;
using ERP.Domain.Purchasing;

namespace ERP.Application.Purchasing.PurchaseOrders;

public sealed record PurchaseOrderListItemDto(
    Guid Id,
    string PoNo,
    Guid SupplierId,
    string SupplierCode,
    string SupplierName,
    DateTime OrderDate,
    DateTime? ExpectedDate,
    DocumentStatus Status,
    PurchaseOrderReceiptProgressStatus ReceiptProgressStatus,
    int LineCount,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
