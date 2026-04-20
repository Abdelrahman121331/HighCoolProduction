# Business Document — HighCool ERP

## System Goal

Build a production-ready operational ERP that manages procurement, inventory, supplier and customer statements, sales, collections, payments, shortage control, supplier commission tracking, employee advances, and payroll.

The system is operations-first and must preserve:

* correct stock
* traceable balances
* auditable business effects
* server-side posting integrity

## Core Master Data

The initial master data scope includes:

* Suppliers
* Warehouses
* Units of measure
* Global UOM conversions
* Items

## Item Rules

Items are the product and component catalog. Components are not a standalone module.

Approved rules:

* Components are modeled only as child rows inside `Item`
* Components are created, updated, and removed only through item workflows
* A database table such as `item_components` may exist, but it is strictly a child table of `items`
* An item must store:
  * `base_uom_id`
  * `is_sellable`
  * `has_components`
* A component row must store:
  * `item_id`
  * `component_item_id`
  * `quantity`
  * `uom_id`

Operational expectations:

* An item may be sellable, non-sellable, or sellable with components
* A component item is just another item in the catalog
* No self-referencing component rows are allowed
* Duplicate component rows for the same item are not allowed
* Component quantity must be greater than zero

## UOM Rules

UOM conversions are global and are never item-specific.

Approved rules:

* No `item_id` is allowed in UOM conversion design
* A conversion is defined only between two UOMs
* Each conversion stores:
  * `from_uom_id`
  * `to_uom_id`
  * `factor`
  * `rounding_mode`

Operational expectations:

* Conversions are shared by all items
* A conversion must exist when an item component row uses a UOM that differs from the component item's base UOM
* Duplicate active conversion pairs are not allowed
* `from_uom_id` and `to_uom_id` must be different

## Inventory and Statement Principles

The following rules remain mandatory:

* The server is the source of truth
* Posting is always server-side
* Posted documents are immutable
* Stock is derived from stock ledger entries only
* Statements are generated from posted business documents only
* No manual statement entries
* Corrections happen through cancel, reversal, return, or adjustment flows

## UI Expectations

Master data UX must support:

* item maintenance with an embedded components grid
* global UOM conversion maintenance through dedicated conversion screens
* responsive desktop and mobile layouts
* search, filter, pagination, loading, empty, and error states
