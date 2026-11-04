# Proposal

## Why

Drone lending is currently tracked ad hoc, so it is unclear which drones are out, which student
group has them, and how many batteries and propellers went with a loan. The project's backend and
frontend are still empty scaffolds, so this change establishes the first working Drone Lending
System: a single source of truth for drones, student groups, accessories and the full lending
history.

## What Changes

- Introduce drone registration and listing with a name, description and a derived status of
  `Available` or `Loaned`.
- Introduce student group creation so groups can be selected as borrowers.
- Introduce drone loans: a loan references exactly one available drone and one student group, and
  records battery and propeller quantities as integer loan items (`Battery` / `Propeller`), each
  `>= 0`. Accessories are quantities only - no stock ledger, no individual battery or propeller
  identifiers, and no difference tracking on return.
- Enforce the domain rules from the project context: a drone has at most one active loan, only
  available drones can be loaned, returning a loan makes the drone available again, and accessory
  quantities can never be negative.
- Add a return operation that closes a loan, stamps the return date and preserves the loan as
  history. Closed loans are never deleted or overwritten.
- Show active loans and preserve returned loans in the loan history.
- Deliver an Angular UI covering drones, student groups, creating a loan, returning a loan and
  browsing loan history, backed by the ASP.NET Core API with EF Core and SQLite persistence.
- Remove the template weather-forecast endpoint and the default Angular placeholder page, which
  serve no purpose in this system.
- Explicitly out of scope: authentication, user accounts, roles, per-battery tracking, accessory
  inventory stock, reservations and multi-drone loans.

## Capabilities

### New Capabilities

- `drone-management`: drone registry and availability state - which drones exist, their
  identifiers and whether they are available for lending.
- `student-group-management`: student group registry - which groups exist and can borrow drones.
- `drone-lending`: the loan lifecycle - creating a loan with accessory quantities, enforcing the
  single-active-loan and availability rules, returning a loan, and exposing loan history.

### Modified Capabilities

None. No specs exist in the project yet.

## Impact

- Backend `backend/DroneInventory.Api`: EF Core `DbContext` with SQLite, domain entities
  (`Drone`, `StudentGroup`, `Loan`, `LoanItem`), minimal API endpoints for drone registration and
  listing, student group selection, loans, returns, active loans and history, and removal of the
  weather-forecast sample.
- Frontend `frontend/drone-inventory-ui`: routes, feature components and an HTTP service layer for
  drone and student group creation, loans, returns and history; the placeholder page is replaced
  by application navigation.
- Dependencies: EF Core SQLite provider added to the API project. No new frontend runtime
  dependencies; Angular's built-in `HttpClient` is used.
- Developer experience: SQLite database file created locally plus EF Core migrations, or
  `EnsureCreated` for the first iteration.
