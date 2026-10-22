# Tasks

## 1. Backend foundation and persistence

- [ ] 1.1 Add `Microsoft.EntityFrameworkCore.Sqlite` and `Microsoft.EntityFrameworkCore.Design` to `DroneInventory.Api.csproj`; verify `dotnet restore` and `dotnet build backend/DroneInventory.Api` succeed with no warnings
- [ ] 1.2 Add a local tool manifest and install `dotnet-ef`; verify `dotnet tool run dotnet-ef --version` prints a 10.x version and `dotnet tool restore` works from a clean clone
- [ ] 1.3 Implement `Drone`, `StudentGroup`, `Loan` and `LoanItem` entities plus `DroneInventoryDbContext` with unique indexes on inventory number and group name, a unique `(LoanId, Type)` index, and a partial unique index on `Loan.DroneId` filtered to active loans; verify `dotnet ef migrations script` output contains all three unique indexes and the `WHERE "ReturnedOn" IS NULL` filter
- [ ] 1.4 Configure the SQLite connection string, generate the initial migration and apply it with `Database.Migrate()` on startup; verify running the API creates `drone-lending.db` and `sqlite3 drone-lending.db ".tables"` lists Drones, StudentGroups, Loans and LoanItems
- [ ] 1.5 Delete the weather-forecast sample and expose the app type for testing; verify `GET /weatherforecast` returns 404 and `GET /openapi/v1.json` still returns a document
- [ ] 1.6 Create `backend/DroneInventory.Api.Tests` (xUnit, `Microsoft.AspNetCore.Mvc.Testing`) with a `WebApplicationFactory<Program>` that uses a temp-file SQLite database and a fixed `TimeProvider`; verify `dotnet test backend/DroneInventory.Api.Tests` runs one smoke test asserting `GET /api/drones` returns 200 and an empty array
- [ ] 1.7 Document backend prerequisites, `dotnet tool restore`, migration and run commands in `README.md`; verify every documented command runs as written on a fresh shell

## 2. Drones API

- [ ] 2.1 Add drone DTOs and `GET/POST /api/drones`, `GET/PUT/DELETE /api/drones/{id}` with the derived `Available`/`OnLoan` status; verify integration tests cover create-then-list, create-then-get and update-then-list
- [ ] 2.2 Enforce required inventory number and model, trimmed values, unique inventory number and 404 for unknown ids, returning `400`/`409` problem details; verify tests for empty fields, duplicate inventory number and update-to-taken-number leave stored data unchanged
- [ ] 2.3 Reject deletion of a drone referenced by any loan (`409`) and allow deletion of an unused drone (`204`); verify tests for both paths, including that history survives the rejected delete
- [ ] 2.4 Support `GET /api/drones?status=available|onloan` for the loan form's drone picker; verify a test returns only available drones when the filter is set and rejects an unknown status value with `400`

## 3. Student groups API

- [ ] 3.1 Add student group DTOs and `GET/POST /api/student-groups`, `PUT/DELETE /api/student-groups/{id}`; verify integration tests cover create-then-list, rename and 404 for unknown ids
- [ ] 3.2 Reject empty names and duplicate names with `400`/`409`; verify tests cover both and confirm the stored name is unchanged after a rejected rename
- [ ] 3.3 Reject deletion of a group referenced by any loan and allow deletion of an unused group; verify tests for both paths

## 4. Loan lifecycle API

- [ ] 4.1 Implement `LoanService.Create` enforcing existing group, existing and available drone, and server-set loan date through the injected `TimeProvider`, inside a transaction; verify tests for a successful loan, unknown group, unknown drone and already-loaned drone
- [ ] 4.2 Validate accessory items as `Battery`/`Propeller` with integer quantities `>= 0`, at most one entry per type, and allow omitting them; verify tests for accepted quantities, absent accessories, negative quantity (`400`) and duplicate accessory type (`400`)
- [ ] 4.3 Guarantee at most one active loan per drone, including when two creations for the same drone are attempted in sequence; verify the second request returns `409` and only one active loan exists, and that a duplicate-key `DbUpdateException` is mapped to `409`
- [ ] 4.4 Implement `POST /api/loans/{id}/return` setting the server return date and freeing the drone; verify tests assert `state` becomes `Returned`, the drone becomes `Available`, a second return returns `409` and keeps the original return date
- [ ] 4.5 Implement `GET /api/loans` and `GET /api/loans/{id}` as loan history with `state`, `droneId` and `studentGroupId` filters, exposing drone, group, accessory quantities, loan date, state and return date; verify tests cover each filter, the full history after a return, and unchanged loan data across return

## 5. Frontend foundation

- [ ] 5.1 Add `provideHttpClient()` to `app.config.ts`, hand-written models for drone, student group, loan and loan item, and `DroneApiService`/`StudentGroupApiService`/`LoanApiService`; verify Vitest specs with `provideHttpClientTesting` assert URLs, methods, bodies and enum string mapping
- [ ] 5.2 Replace the placeholder page with a navigation shell and lazy routes for `/drones`, `/student-groups`, `/loans` and `/loans/new`, and add `proxy.conf.json` mapping `/api` to `http://localhost:5214` in `angular.json`; verify `ng build` succeeds, the app spec no longer asserts the placeholder heading, and `ng serve` forwards an `/api/drones` request to the running API
- [ ] 5.3 Document frontend prerequisites, `npm start` and `npm test` in `README.md`; verify the documented commands run as written

## 6. Drones and student group screens

- [ ] 6.1 Build the drone list with status badges plus create/edit/delete forms that surface `400`/`409` problem details; verify component tests render a created drone, show `OnLoan` for a loaned drone and display the server message when a delete is rejected
- [ ] 6.2 Build the student group list with create/edit/delete forms and duplicate-name error handling; verify component tests cover listing, creating and the rejected-delete message

## 7. Loan screens

- [ ] 7.1 Build the create-loan page with available-drone and student-group selectors and numeric battery/propeller inputs (`min="0"`, integers only), submitting through `LoanApiService`; verify tests reject negative and non-integer quantities before submit and post the expected payload, and that a `409` from the API is shown to the user
- [ ] 7.2 Build the loan history page with state, drone and group filters, accessory quantities, dates and a return action that refreshes the row; verify tests cover rendering mixed active/returned loans, filtering by state and the return action updating the displayed state

## 8. Integration verification

- [ ] 8.1 Run `dotnet build` and `dotnet test` for the API and test project; verify all tests pass with no skipped scenarios from `specs/`
- [ ] 8.2 Run `npm run build` and `npm test` in `frontend/drone-inventory-ui`; verify both succeed
- [ ] 8.3 Start the API and the Angular dev server, then walk the full flow over HTTP (create drone, create group, create loan with 2 batteries and 4 propellers, read history, return loan, confirm drone available); verify each response matches the specs and that the UI renders the same flow in the browser

## Workflow follow-up

- Archive the change once implementation and the project's review are complete, then verify the archived specs.
