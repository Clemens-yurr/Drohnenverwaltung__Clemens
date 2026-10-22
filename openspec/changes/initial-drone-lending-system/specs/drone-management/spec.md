# Spec Delta

## Purpose

Maintains the school's drone fleet - which drones exist, how they are identified, and whether each
one is currently available for lending.

## ADDED Requirements

### Requirement: Drone registry

The system SHALL let staff create, list, view, update and delete drones. Each drone SHALL store a
unique inventory number and a model name.

#### Scenario: Creating a drone

- **WHEN** staff submit an inventory number and a model for a new drone
- **THEN** the drone is stored and appears in the drone list

#### Scenario: Creating a drone without required data

- **WHEN** staff submit a drone without an inventory number or without a model
- **THEN** the request is rejected with a validation error and no drone is created

#### Scenario: Registering a duplicate inventory number

- **WHEN** staff submit an inventory number that already belongs to another drone
- **THEN** the request is rejected with a validation error and no drone is created

### Requirement: Drone details can be edited

The system SHALL allow the inventory number and model of an existing drone to be changed, provided
the inventory number remains unique.

#### Scenario: Updating a drone model

- **WHEN** staff change the model of an existing drone
- **THEN** the drone list shows the new model for that drone

#### Scenario: Updating to a taken inventory number

- **WHEN** staff change a drone's inventory number to one used by another drone
- **THEN** the request is rejected and the drone keeps its previous inventory number

### Requirement: Drone availability status

Each drone SHALL expose exactly one status: `Available` when it has no active loan, `OnLoan` when
an active loan exists. The status SHALL be derived from loans and SHALL NOT be editable directly.

#### Scenario: Drone without an active loan

- **WHEN** staff view a drone that has no active loan
- **THEN** the drone is reported as Available

#### Scenario: Drone with an active loan

- **WHEN** staff view a drone that is the subject of an active loan
- **THEN** the drone is reported as OnLoan

### Requirement: Deleting drones

The system SHALL allow a drone to be deleted only when it is not part of any loan, so that loan
history is never destroyed.

#### Scenario: Deleting an unused drone

- **WHEN** staff delete a drone that is not referenced by any loan
- **THEN** the drone no longer appears in the drone list

#### Scenario: Deleting a drone with loan involvement

- **WHEN** staff delete a drone that has an active loan or any past loan
- **THEN** the request is rejected and the drone plus its loan history remain unchanged
