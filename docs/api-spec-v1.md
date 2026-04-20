# API Spec v1 — Procurement and Inventory

## Purchase Orders

### `GET /api/purchase-orders`

Lists purchase orders.

Optional query parameters:

* `search`

### `GET /api/purchase-orders/{id}`

Returns one purchase order with nested lines and computed receipt progress.

### `GET /api/purchase-orders/{id}/available-lines-for-receipt`

Returns posted PO lines with remaining receivable quantity greater than zero.

### `POST /api/purchase-orders`

Creates a purchase order draft.

Request body:

```json
{
  "poNo": "PO-20260420-0001",
  "supplierId": "guid",
  "orderDate": "2026-04-20T00:00:00.000Z",
  "expectedDate": "2026-04-25T00:00:00.000Z",
  "notes": "Expected supplier delivery",
  "lines": [
    {
      "lineNo": 1,
      "itemId": "guid",
      "orderedQty": 10.0,
      "uomId": "guid",
      "notes": "Main ordered item"
    }
  ]
}
```

### `PUT /api/purchase-orders/{id}`

Updates a purchase order draft.

### `POST /api/purchase-orders/{id}/post`

Posts a draft purchase order.

### `POST /api/purchase-orders/{id}/cancel`

Cancels a posted purchase order when no posted receipts already exist.

## Purchase Receipts

### `GET /api/purchase-receipts`

Lists purchase receipts.

Optional query parameters:

* `search`

### `GET /api/purchase-receipts/{id}`

Returns one purchase receipt with nested lines and auto-filled component rows. Each component row includes system-derived `expectedQty` plus editable `actualReceivedQty`.

### `POST /api/purchase-receipts`

Creates a purchase receipt draft.

Request body:

```json
{
  "receiptNo": "PR-20260420-0001",
  "supplierId": "guid",
  "warehouseId": "guid",
  "purchaseOrderId": "guid",
  "receiptDate": "2026-04-20T00:00:00.000Z",
  "notes": "Receipt capture",
  "lines": [
    {
      "lineNo": 1,
      "purchaseOrderLineId": "guid",
      "itemId": "guid",
      "orderedQtySnapshot": 10.0,
      "receivedQty": 6.0,
      "uomId": "guid",
      "notes": "Partial receipt",
      "components": [
        {
          "componentItemId": "guid",
          "expectedQty": 12.0,
          "actualReceivedQty": 11.0,
          "uomId": "guid",
          "shortageReasonCodeId": "guid",
          "notes": "Short on one component"
        }
      ]
    }
  ]
}
```

### `PUT /api/purchase-receipts/{id}`

Updates a draft purchase receipt.

### `POST /api/purchase-receipts/{id}/post`

Posts a draft purchase receipt.

Behavior:

* receipt line components are derived from the selected item's BOM components
* expected component quantities are calculated as `receivedQty x item component quantity`
* actual component quantities default to expected quantities and remain editable
* shortage reason is required only when actual component quantity is below expected quantity
* only `Draft` receipts can be posted
* posting is idempotent
* linked PO quantities cannot exceed remaining posted PO quantity
* posting creates stock ledger entries
* posting creates shortage ledger entries when actual components are below expected BOM quantities

## Shortage Reason Codes

### `GET /api/shortage-reason-codes`

Lists active shortage reason codes for purchase receipt shortage capture.

## Validation Responses

The API returns:

* `400` for business rule violations
* `409` for duplicate document numbers or duplicate entity constraints
* `404` when the target record does not exist
