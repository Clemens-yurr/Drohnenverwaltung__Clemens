# Spec Delta

## Purpose

Owns the lending lifecycle: handing an available drone and accessory quantities to a student group,
enforcing the lending rules, taking the loan back, and keeping every loan as retrievable history.

## ADDED Requirements

### Requirement: Creating a loan

The system SHALL create a loan from exactly one drone, exactly one existing student group and an
optional set of accessory quantities. The loan records the loan date and starts in the active state.

#### Scenario: Loaning a drone to a group

- **WHEN** staff create a loan for an available drone and an existing student group
- **THEN** the loan is stored as active with the current date as loan date

#### Scenario: Loaning to an unknown group

- **WHEN** staff create a loan that references a student group that does not exist
- **THEN** the request is rejected and no loan is created

#### Scenario: Loaning an unknown drone

- **WHEN** staff create a loan that references a drone that does not exist
- **THEN** the request is rejected and no loan is created

### Requirement: Only available drones can be loaned

The system SHALL reject a new loan for a drone whose status is not `Available`.

#### Scenario: Loaning a drone that is already out

- **WHEN** staff create a loan for a drone that already has an active loan
- **THEN** the request is rejected and the existing loan is unchanged

### Requirement: A drone has at most one active loan

A drone SHALL never be part of more than one active loan at the same time, regardless of which
student group borrows it.

#### Scenario: Two groups request the same drone

- **WHEN** a loan for a drone is active and staff request another loan for that same drone
- **THEN** only the first loan stays active and the second request is rejected

### Requirement: Accessory quantities

Batteries and propellers SHALL be recorded per loan as integer quantities of at least zero, each
tagged with the accessory type `Battery` or `Propeller`. Negative quantities SHALL be rejected.
Accessories are quantities only: no stock level is tracked and no item is identified individually.

#### Scenario: Recording accessory quantities

- **WHEN** staff create a loan with 2 batteries and 4 propellers
- **THEN** the loan reports an accessory entry of 2 for Battery and 4 for Propeller

#### Scenario: Omitting accessories

- **WHEN** staff create a loan without any battery or propeller quantities
- **THEN** the loan is accepted and reports no accessory entries

#### Scenario: Negative accessory quantity

- **WHEN** staff create a loan with a battery or propeller quantity below zero
- **THEN** the request is rejected and no loan is created

### Requirement: Returning a loan

The system SHALL let staff return an active loan. Returning SHALL record the return date, close the
loan and make its drone `Available` again.

#### Scenario: Returning an active loan

- **WHEN** staff return an active loan
- **THEN** the loan is closed with a return date and its drone is reported as Available

#### Scenario: Returning an already returned loan

- **WHEN** staff return a loan that is already closed
- **THEN** the request is rejected and the loan keeps its original return date

### Requirement: Loan history

The system SHALL retain every loan, active and returned, and SHALL expose them as loan history that
can be filtered by drone, student group and loan state. Returning a loan SHALL NOT remove or alter
its recorded drone, student group or accessory quantities.

#### Scenario: Viewing loan history

- **WHEN** staff open the loan history
- **THEN** all loans are listed with their drone, student group, accessory quantities, loan date,
  state and return date

#### Scenario: Filtering history by state

- **WHEN** staff filter the loan history by the returned state
- **THEN** only closed loans are listed

#### Scenario: History after a return

- **WHEN** staff return a loan and reopen the loan history
- **THEN** the loan is still listed with the same drone, student group and accessory quantities, now
  shown as returned
