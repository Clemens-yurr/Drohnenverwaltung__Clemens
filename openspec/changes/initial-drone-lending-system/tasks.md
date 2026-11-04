# Tasks

## 1. Backend foundation and persistence

- [x] 1.1 Add `Microsoft.EntityFrameworkCore.Sqlite` and `Microsoft.EntityFrameworkCore.Design` to `DroneInventory.Api.csproj`; verify `dotnet restore` and `dotnet build backend/DroneInventory.Api` succeed — [#2](https://github.com/Clemens-yurr/Drohnenverwaltung__Clemens/issues/2)
- [ ] 1.2 Add a local tool manifest and install `dotnet-ef`; verify the 10.x tool version and `dotnet tool restore` — [#3](https://github.com/Clemens-yurr/Drohnenverwaltung__Clemens/issues/3)
- [ ] 1.3 Implement `Drone`, `StudentGroup`, `Loan` and `LoanItem` entities plus `DroneInventoryDbContext`; enforce one active loan per drone and at most one accessory entry per type per loan — [#4](https://github.com/Clemens-yurr/Drohnenverwaltung__Clemens/issues/4)
- [ ] 1.4 Configure SQLite, generate and apply the initial migration at startup; verify the database contains Drones, StudentGroups, Loans and LoanItems — [#5](https://github.com/Clemens-yurr/Drohnenverwaltung__Clemens/issues/5)
- [x] 1.5 Remove the weather forecast sample, expose `Program` for testing, and create a test project with a smoke test for an empty `GET /api/drones` — [#6](https://github.com/Clemens-yurr/Drohnenverwaltung__Clemens/issues/6)
- [ ] 1.6 Document backend prerequisites, tool restore, migration and run commands in `README.md` — [#7](https://github.com/Clemens-yurr/Drohnenverwaltung__Clemens/issues/7)

## 2. Drone API

- [ ] 2.1 Add drone DTOs and `GET/POST /api/drones`; return name, description and derived `Available`/`Loaned` status; test create, list and status derivation — [#8](https://github.com/Clemens-yurr/Drohnenverwaltung__Clemens/issues/8)
- [ ] 2.2 Validate required drone name and description and return `400` problem details for invalid input; test valid and invalid requests — [#9](https://github.com/Clemens-yurr/Drohnenverwaltung__Clemens/issues/9)

## 3. Student group API

- [ ] 3.1 Add student group DTOs and `GET/POST /api/student-groups` for creating groups and selecting them in the loan form; validate required names and test create/list behavior — [#10](https://github.com/Clemens-yurr/Drohnenverwaltung__Clemens/issues/10)

## 4. Loan lifecycle API

- [ ] 4.1 Implement loan creation for an existing group and available drone, with a server-set loan date and transaction; test success and unknown group/drone — [#11](https://github.com/Clemens-yurr/Drohnenverwaltung__Clemens/issues/11)
- [ ] 4.2 Validate optional `Battery`/`Propeller` items as integer quantities `>= 0`, with at most one entry per type; test omitted, valid, negative and duplicate items — [#12](https://github.com/Clemens-yurr/Drohnenverwaltung__Clemens/issues/12)
- [ ] 4.3 Guarantee at most one active loan per drone, including a database constraint; test that a second loan is rejected and only one active loan remains — [#13](https://github.com/Clemens-yurr/Drohnenverwaltung__Clemens/issues/13)
- [ ] 4.4 Implement `POST /api/loans/{id}/return` with server-set return date, marking the loan `Returned` and freeing its drone; test second return rejection and preserved original return date — [#14](https://github.com/Clemens-yurr/Drohnenverwaltung__Clemens/issues/14)
- [ ] 4.5 Implement active loan listing and complete loan history with drone, student group, accessory quantities, loan date, state and return date; test active and returned views — [#15](https://github.com/Clemens-yurr/Drohnenverwaltung__Clemens/issues/15)

## 5. Frontend foundation

- [ ] 5.1 Add `provideHttpClient()`, models and `DroneApiService`, `StudentGroupApiService` and `LoanApiService`; test URLs, methods, request bodies and enum strings — [#16](https://github.com/Clemens-yurr/Drohnenverwaltung__Clemens/issues/16)
- [ ] 5.2 Replace the placeholder with navigation and routes for `/drones`, `/student-groups`, `/loans` and `/loans/new`; configure the `/api` development proxy and verify the app builds — [#17](https://github.com/Clemens-yurr/Drohnenverwaltung__Clemens/issues/17)
- [ ] 5.3 Document frontend prerequisites, `npm start` and `npm test` in `README.md` — [#18](https://github.com/Clemens-yurr/Drohnenverwaltung__Clemens/issues/18)

## 6. Drone and student group screens

- [ ] 6.1 Build the drone list and create form with `Available`/`Loaned` status; test listing, creation and validation messages — [#19](https://github.com/Clemens-yurr/Drohnenverwaltung__Clemens/issues/19)
- [ ] 6.2 Build student group list and create form; test listing and creation — [#20](https://github.com/Clemens-yurr/Drohnenverwaltung__Clemens/issues/20)

## 7. Loan screens

- [ ] 7.1 Build the loan form with available drone and student group selectors and nonnegative integer battery/propeller inputs; test validation, submitted payload and API conflict messages — [#21](https://github.com/Clemens-yurr/Drohnenverwaltung__Clemens/issues/21)
- [ ] 7.2 Build active loan and history views showing groups, drones, accessories and dates, with a return action that refreshes the loan state; test active/returned rendering and return behavior — [#22](https://github.com/Clemens-yurr/Drohnenverwaltung__Clemens/issues/22)

## 8. Integration verification

- [ ] 8.1 Run `dotnet build` and `dotnet test` for the API and test project; verify all specified scenarios are covered — [#23](https://github.com/Clemens-yurr/Drohnenverwaltung__Clemens/issues/23)
- [ ] 8.2 Run `npm run build` and `npm test` in `frontend/drone-inventory-ui` — [#24](https://github.com/Clemens-yurr/Drohnenverwaltung__Clemens/issues/24)
- [ ] 8.3 Run the complete flow: create drone, create group, loan with 2 batteries and 4 propellers, view active loan/history, return it, and confirm the drone is available; verify HTTP responses and UI — [#25](https://github.com/Clemens-yurr/Drohnenverwaltung__Clemens/issues/25)

## Workflow follow-up

- Archive the change only after implementation, tests, reviews and merges are complete; then verify the archived specs.
