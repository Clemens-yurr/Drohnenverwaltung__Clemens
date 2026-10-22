# Spec Delta

## Purpose

Maintains the student groups that may borrow drones, giving every loan a named, reusable borrower
without introducing user accounts.

## ADDED Requirements

### Requirement: Student group registry

The system SHALL let staff create, list, view, update and delete student groups. Each group SHALL
have a unique name.

#### Scenario: Creating a student group

- **WHEN** staff submit a name for a new student group
- **THEN** the group is stored and available for selection when creating a loan

#### Scenario: Creating a group without a name

- **WHEN** staff submit a student group with an empty name
- **THEN** the request is rejected with a validation error and no group is created

#### Scenario: Duplicate group name

- **WHEN** staff submit a group name that is already used by another group
- **THEN** the request is rejected with a validation error and no group is created

### Requirement: Student group details can be edited

The system SHALL allow the name of an existing student group to be changed, provided the name
remains unique.

#### Scenario: Renaming a group

- **WHEN** staff rename an existing student group to an unused name
- **THEN** the new name is shown in the group list

#### Scenario: Renaming to an existing name

- **WHEN** staff rename a group to a name used by another group
- **THEN** the request is rejected and the group keeps its previous name

### Requirement: Deleting student groups

The system SHALL allow a student group to be deleted only when it is not part of any loan, so that
the borrower of historical loans remains identifiable.

#### Scenario: Deleting an unused group

- **WHEN** staff delete a student group that is not referenced by any loan
- **THEN** the group no longer appears in the group list

#### Scenario: Deleting a group with loan history

- **WHEN** staff delete a student group that has an active or past loan
- **THEN** the request is rejected and the group plus its loan history remain unchanged
