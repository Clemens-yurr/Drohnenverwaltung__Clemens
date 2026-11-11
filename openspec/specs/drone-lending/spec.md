# Drone Lending

## Purpose

Describes creating, returning and viewing drone loans, including the quantities of batteries and propellers issued.

## Requirements

### Requirement: Create an active loan

The system SHALL create an active loan for exactly one available drone and one existing student group, recording the server's current date as the loan date.

#### Scenario: Loan an available drone

- **WHEN** staff create a loan for an available drone and an existing group
- **THEN** the system creates an active loan with the current loan date

#### Scenario: Reject a loan for a drone that is already loaned

- **WHEN** staff create a loan for a drone with an active loan
- **THEN** the request is rejected and the existing loan is unchanged

### Requirement: Allow at most one active loan per drone

A drone SHALL have at most one active loan at any time.

#### Scenario: Prevent a second active loan

- **GIVEN** a drone already has an active loan
- **WHEN** staff attempt to create another loan for the same drone
- **THEN** the second loan is rejected

### Requirement: Record accessory quantities

A loan SHALL record battery and propeller quantities as nonnegative integers. Accessories SHALL be tracked only as quantities, not as individually identified items.

#### Scenario: Record accessories

- **WHEN** staff create a loan with 2 batteries and 4 propellers
- **THEN** the loan records those quantities

#### Scenario: Reject a negative quantity

- **WHEN** staff create a loan with a negative battery or propeller quantity
- **THEN** the request is rejected and no loan is created

### Requirement: Show active loans

The system SHALL list active loans with the student group, drone, battery quantity, propeller quantity and loan date.

#### Scenario: View active loans

- **WHEN** staff view active loans
- **THEN** the system shows the group, drone, accessory quantities and loan date for each active loan

### Requirement: Return a loan and record the return date

The system SHALL let staff return an active loan, record the server's current date, mark the loan as `Returned` and make its drone `Available` again.

#### Scenario: Return an active loan

- **WHEN** staff return an active loan
- **THEN** the loan is marked `Returned`, its return date is recorded and its drone becomes `Available`

### Requirement: Preserve loan history

The system SHALL retain returned loans and show them in loan history with the drone, student group, accessory quantities, loan date, state and return date.

#### Scenario: View loan history after return

- **GIVEN** a loan has been returned
- **WHEN** staff view loan history
- **THEN** the loan remains listed with its original drone, group and accessory quantities, its returned state and return date
