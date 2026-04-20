# Posting Matrix

## Purchase Order

### Draft Save

Actions:

* `POST /api/purchase-orders`
* `PUT /api/purchase-orders/{id}`

Effects:

* persists PO header and lines
* no stock effect
* no shortage effect
* no financial effect
* status remains `Draft`

### Post

Action:

* `POST /api/purchase-orders/{id}/post`

Effects:

* validates draft PO
* status changes to `Posted`
* PO becomes immutable except through cancel
* no stock ledger effect
* no shortage ledger effect

### Cancel

Action:

* `POST /api/purchase-orders/{id}/cancel`

Effects:

* status changes to `Canceled`
* blocked when posted receipts already exist for the PO
* no stock ledger effect

## Purchase Receipt

### Draft Save

Actions:

* `POST /api/purchase-receipts`
* `PUT /api/purchase-receipts/{id}`

Effects:

* persists receipt header, lines, and auto-filled component rows
* stores PO linkage and ordered snapshot when linked
* recalculates `expected_qty` for each component row from `received_qty x item BOM quantity`
* defaults `actual_received_qty` to `expected_qty` when a component row has not been edited yet
* no stock ledger effect
* no shortage ledger effect
* status remains `Draft`

### Post

Action:

* `POST /api/purchase-receipts/{id}/post`

Preconditions:

* receipt exists
* receipt status is `Draft`
* supplier exists and is active
* warehouse exists and is active
* at least one line exists
* all item and UOM references resolve
* required global UOM conversions resolve
* linked PO, when supplied, is `Posted`
* linked PO supplier matches receipt supplier
* linked PO receipt quantities do not exceed remaining posted PO quantity
* actual component rows match the BOM component set for BOM items
* shortage reason exists for every positive shortage
* shortage rows are based on persisted `expected_qty` vs `actual_received_qty` on the receipt line component rows

Posting effects:

* status changes from `Draft` to `Posted`
* one stock ledger `IN` row is written per receipt line
* expected component quantities are expanded from the item BOM using `received_qty`
* actual components are compared against expected quantities
* shortage ledger rows are written only for positive shortages

Idempotency:

* reposting an already posted receipt returns the current posted document
* duplicate stock rows are guarded by receipt status plus unique stock ledger indexing
