# Architecture — HighCool ERP

## Principles

* Modular monolith
* ASP.NET Core Web API backend
* EF Core persistence
* React + TypeScript frontend
* Server as source of truth
* Draft-only offline support

## Backend Shape

Business logic lives in services, not endpoint handlers.

Current aggregate direction:

* `Item` owns child `ItemComponent` rows
* `UomConversion` is global master data
* `PurchaseOrder` owns `PurchaseOrderLine`
* `PurchaseReceipt` owns `PurchaseReceiptLine`
* `PurchaseReceiptLine` owns actual `PurchaseReceiptLineComponent` rows

## Document Model

### Purchase Order

Document status:

* `Draft`
* `Posted`
* `Canceled`

Receipt progress is computed from posted receipts:

* `NotReceived`
* `PartiallyReceived`
* `FullyReceived`

The computed progress is separate from document status so the system can keep immutable posting semantics while still exposing open receipt progress.

### Purchase Receipt

Document status:

* `Draft`
* `Posted`
* `Canceled`

Current implementation uses:

* optional `purchase_order_id` on header
* optional `purchase_order_line_id` on line
* optional manual receipts without PO linkage

## Service Responsibilities

### PurchaseOrderService

* draft create, update, get, list
* validates supplier, line items, line UOMs
* computes receipt progress from posted receipts
* exposes available remaining PO lines for receipt creation

### PurchaseOrderPostingService

* validates draft PO
* marks PO as `Posted`

### PurchaseOrderCancellationService

* cancels posted PO
* blocks cancel when posted receipts already exist

### PurchaseReceiptService

* draft create, update, get, list
* validates PO linkage when supplied
* enforces `ordered_qty_snapshot` from PO line
* blocks linked receipt quantities beyond remaining posted PO quantity

### PurchaseReceiptPostingService

* validates draft receipt
* keeps posting idempotent
* writes stock ledger rows
* runs shortage detection
* marks receipt as `Posted`

### StockLedgerService

* converts document quantity to base UOM
* writes append-only stock ledger entries
* carries traceability through source document and line references

### ShortageDetectionService

* loads expected components from item BOM
* computes expected component quantity from posted receipt quantity
* compares expected against actual component rows
* creates shortage ledger rows only for positive shortages

## Persistence Design

Key tables for this slice:

* `purchase_orders`
* `purchase_order_lines`
* `purchase_receipts`
* `purchase_receipt_lines`
* `purchase_receipt_line_components`
* `stock_ledger_entries`
* `shortage_ledger_entries`

Important constraints:

* stock and shortage ledgers are append-only
* posted document data is never mutated into a different business effect
* PO-to-receipt traceability is stored directly in receipt header and line rows
