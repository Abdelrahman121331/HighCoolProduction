using ERP.Domain.Common;
using ERP.Domain.MasterData;

namespace ERP.Domain.Purchasing;

public sealed class PurchaseOrderLine : AuditableEntity
{
    public Guid PurchaseOrderId { get; set; }

    public PurchaseOrder? PurchaseOrder { get; set; }

    public int LineNo { get; set; }

    public Guid ItemId { get; set; }

    public Item? Item { get; set; }

    public decimal OrderedQty { get; set; }

    public Guid UomId { get; set; }

    public Uom? Uom { get; set; }

    public string? Notes { get; set; }
}
