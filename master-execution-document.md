# Master Execution Document — HighCool ERP

## Execution Principles

1. Build features end-to-end
2. Keep server-side correctness ahead of convenience
3. Preserve auditability and traceability
4. Keep documentation aligned with implementation

## Current Master Data Decisions

These decisions are mandatory for all ongoing work:

### Components

* Components are not a separate module
* Components are child rows of items only
* Components are maintained only through item create and update flows
* No standalone components CRUD is allowed in API or UI

### UOM Conversions

* UOM conversions are global
* No `item_id` is allowed in conversion schema or API contracts
* Each conversion stores `from_uom_id`, `to_uom_id`, `factor`, and `rounding_mode`

## Phase 1 Scope

The active master-data phase includes:

* Suppliers
* Warehouses
* UOMs
* Global UOM conversions
* Items with embedded component rows

Definition of done for this slice:

* documents updated with no contradictions
* backend entities and migrations aligned
* item APIs accept and return component rows
* global UOM conversion APIs work
* frontend item form manages component rows inline
* frontend conversion screens manage global conversions
* tests cover validation and basic API behavior

## Delivery Rule

Any future item or measurement work must continue from this model and must not reintroduce:

* standalone Components module design
* item-specific UOM conversion design
