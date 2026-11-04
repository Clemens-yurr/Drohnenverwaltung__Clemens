# Spec Delta

## Purpose

Describes the drone data staff can register and view, including availability derived from active loans.

## ADDED Requirements

### Requirement: Create and list drones

The system SHALL let staff create drones with a name and description and list drones with their name, status and description.

#### Scenario: Create a drone

- **WHEN** staff create a drone with a name and description
- **THEN** the system stores it with an identifier, name and description

#### Scenario: List drone details and availability

- **WHEN** staff list drones
- **THEN** every drone is shown with its name, description and status (`Available` or `Loaned`)

### Requirement: Derive drone availability from active loans

A drone SHALL be `Loaned` when it has an active loan and `Available` when it has no active loan. The status SHALL NOT be directly editable.

#### Scenario: Drone without an active loan

- **WHEN** staff view a drone that has no active loan
- **THEN** it is reported as `Available`

#### Scenario: Drone with an active loan

- **WHEN** staff view a drone that has an active loan
- **THEN** it is reported as `Loaned`
