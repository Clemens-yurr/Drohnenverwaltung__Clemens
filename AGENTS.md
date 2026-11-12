# Repository instructions

## Project
Drone Lending System for the SYP project. Requirements are in `requirements.md`; OpenSpec change artifacts are under `openspec/changes/`.

## Architecture
- Angular and TypeScript frontend communicates with the ASP.NET Core Web API through REST/JSON.
- ASP.NET Core 10, C#, Entity Framework Core and SQLite form the backend.
- Keep business rules and validation in the backend. The frontend must not access SQLite directly.

## Domain rules
- A drone can have at most one active loan.
- Only available drones can be loaned.
- Returning a loan frees the drone and preserves the loan history.
- Battery and propeller quantities are nonnegative integers.

## Workflow
- Treat OpenSpec specifications as the behavior source of truth. Update and review specs before changing behavior.
- Work from one GitHub Issue per task and use a branch for that issue. Read `git diff` before committing.
- Do not mark tasks complete unless the implementation and required checks are verified.
- Merge and pass the required checks before archiving an OpenSpec change.
- Skills are stored under `.agents/skills/<name>/SKILL.md`.

## Useful checks
- Backend: `dotnet test backend/DroneInventory.Api.Tests`
- Frontend: `npm test` from `frontend/drone-inventory-ui`
