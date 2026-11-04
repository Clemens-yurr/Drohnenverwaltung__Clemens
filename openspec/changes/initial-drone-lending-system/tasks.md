# Tasks

## 1. Backend foundation and persistence

- [ ] 1.1 Add `Microsoft.EntityFrameworkCore.Sqlite` and `Microsoft.EntityFrameworkCore.Design` to `DroneInventory.Api.csproj`; verify `dotnet restore` and `dotnet build backend/DroneInventory.Api` succeed
- [ ] 1.2 Add a local tool manifest and install `dotnet-ef`; verify the 10.x tool version and `dotnet tool restore`
- [ ] 1.3 Implement `Drone`, `StudentGroup`, `Loan` and `LoanItem` entities plus `DroneInventoryDbContext`; enforce one active loan per drone and at most one accessory entry per type per loan
- [ ] 1.4 Configure SQLite, generate and apply the initial migration at startup; verify the database contains Drones, StudentGroups, Loans and LoanItems
- [ ] 1.5 Remove the weather forecast sample, expose `Program` for testing, and create a test project with a smoke test for an empty `GET /api/drones`
- [ ] 1.6 Document backend prerequisites, tool restore, migration and run commands in `README.md`

## 2. Drone API

- [ ] 2.1 Add drone DTOs and `GET/POST /api/drones`; return name, description and derived `Available`/`Loaned` status; test create, list and status derivation
- [ ] 2.2 Validate required drone name and description and return `400` problem details for invalid input; test valid and invalid requests

## 3. Student group API

- [ ] 3.1 Add student group DTOs and `GET/POST /api/student-groups` for creating groups and selecting them in the loan form; validate required names and test create/list behavior

## 4. Loan lifecycle API

- [ ] 4.1 Implement loan creation for an existing group and available drone, with a server-set loan date and transaction; test success and unknown group/drone
- [ ] 4.2 Validate optional `Battery`/`Propeller` items as integer quantities `>= 0`, with at most one entry per type; test omitted, valid, negative and duplicate items
- [ ] 4.3 Guarantee at most one active loan per drone, including a database constraint; test that a second loan is rejected and only one active loan remains
- [ ] 4.4 Implement `POST /api/loans/{id}/return` with server-set return date, marking the loan `Returned` and freeing its drone; test second return rejection and preserved original return date
- [ ] 4.5 Implement active loan listing and complete loan history with drone, student group, accessory quantities, loan date, state and return date; test active and returned views

## 5. Frontend foundation

- [ ] 5.1 Add `provideHttpClient()`, models and `DroneApiService`, `StudentGroupApiService` and `LoanApiService`; test URLs, methods, request bodies and enum strings
- [ ] 5.2 Replace the placeholder with navigation and routes for `/drones`, `/student-groups`, `/loans` and `/loans/new`; configure the `/api` development proxy and verify the app builds
- [ ] 5.3 Document frontend prerequisites, `npm start` and `npm test` in `README.md`

## 6. Drone and student group screens

- [ ] 6.1 Build the drone list and create form with `Available`/`Loaned` status; test listing, creation and validation messages
- [ ] 6.2 Build student group list and create form; test listing and creation

## 7. Loan screens

- [ ] 7.1 Build the loan form with available drone and student group selectors and nonnegative integer battery/propeller inputs; test validation, submitted payload and API conflict messages
- [ ] 7.2 Build active loan and history views showing groups, drones, accessories and dates, with a return action that refreshes the loan state; test active/returned rendering and return behavior

## 8. Integration verification

- [ ] 8.1 Run `dotnet build` and `dotnet test` for the API and test project; verify all specified scenarios are covered
- [ ] 8.2 Run `npm run build` and `npm test` in `frontend/drone-inventory-ui`
- [ ] 8.3 Run the complete flow: create drone, create group, loan with 2 batteries and 4 propellers, view active loan/history, return it, and confirm the drone is available; verify HTTP responses and UI

## Workflow follow-up

- Archive the change only after implementation, tests, reviews and merges are complete; then verify the archived specs.
