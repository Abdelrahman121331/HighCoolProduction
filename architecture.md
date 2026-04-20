# Architecture — HighCool ERP

## Principles

* Modular monolith
* ASP.NET Core Web API backend
* EF Core persistence
* React + TypeScript frontend
* Server as source of truth
* Draft-only offline support

## Backend Shape

Business logic lives in application and domain services, not controllers or endpoint handlers.

Master data design for this phase:

* `Item` is the aggregate root for item component rows
* `ItemComponent` is a child entity owned through item create and update workflows
* `UomConversion` is a standalone global master-data entity

This means:

* there is no standalone Components module
* there is no standalone Components CRUD surface
* there is no item-scoped UOM conversion design

## Aggregate Boundaries

### Item

Owns:

* item identity and flags
* base UOM reference
* child component rows

Rules enforced in the item workflow:

* self-reference prevention
* duplicate component prevention
* positive component quantities
* required global UOM conversion when a component row uses a non-base UOM

### UOM Conversion

Owns:

* global UOM pair definition
* factor
* rounding mode
* active status

Rules enforced in the conversion workflow:

* both UOMs must exist
* pair members must be different
* duplicate active pairs are rejected

## API Design

The API should expose:

* item create and update requests with embedded component rows
* item get responses with embedded component rows
* global UOM conversion CRUD endpoints

The API should not expose:

* standalone item component CRUD endpoints
* item-specific conversion endpoints

## Frontend Design

The frontend mirrors the backend boundaries:

* item form contains the components grid
* no standalone components navigation or page
* UOM conversions are managed from a dedicated global conversion screen

## Persistence Design

Schema direction for this phase:

* `items`
* `item_components`
* `uoms`
* `uom_conversions`

Important constraints:

* `item_components` is keyed by its parent `item_id`
* `uom_conversions` contains no `item_id`
