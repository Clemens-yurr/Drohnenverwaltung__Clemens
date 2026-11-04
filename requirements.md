# Requirements: Drone Lending System

These ten requirements are the project baseline from the project assignment. The OpenSpec change and implementation must satisfy them without silently adding unrelated domain behavior.

## REQ-001 - Create a drone

The system SHALL let staff create a drone with an identifier, name and description.

### Scenario: Create a drone

- GIVEN the drone does not yet exist
- WHEN staff create a drone with a name and description
- THEN the system stores the drone with its identifier, name and description

## REQ-002 - List drones

The system SHALL list each drone with its name, status (`Available` or `Loaned`) and description. The status SHALL be derived from whether the drone has an active loan.

### Scenario: List available and loaned drones

- GIVEN one drone has no active loan and another drone has an active loan
- WHEN staff list the drones
- THEN the first drone is shown as `Available` and the second as `Loaned`, each with its name and description

## REQ-003 - Create a student group

The system SHALL let staff create a student group with an identifier and name.

### Scenario: Create a student group

- GIVEN the student group does not yet exist
- WHEN staff create a student group with a name
- THEN the system stores the group with its identifier and name

## REQ-004 - Loan a drone

An available drone SHALL be loaned to an existing student group, creating an active loan with a server-set loan date.

### Scenario: Loan an available drone

- GIVEN the drone "TELLO-07" is available
- AND the student group "Group 3" exists
- WHEN staff loan "TELLO-07" to "Group 3"
- THEN the system creates an active loan dated with the current date

## REQ-005 - Prevent duplicate active loans

A drone SHALL have at most one active loan at a time.

### Scenario: Reject a second active loan

- GIVEN "TELLO-07" already has an active loan
- WHEN staff try to loan it to another group
- THEN the request is rejected and the existing loan remains unchanged

## REQ-006 - Record accessory quantities

The system SHALL record battery and propeller quantities on a loan as integers greater than or equal to zero. Accessories SHALL be tracked only as quantities, not as individually identified items.

### Scenario: Loan accessories

- GIVEN an available drone and an existing student group
- WHEN staff create a loan with 4 batteries and 8 propellers
- THEN the loan records 4 batteries and 8 propellers

### Scenario: Reject a negative accessory quantity

- GIVEN an available drone and an existing student group
- WHEN staff create a loan with a negative battery or propeller quantity
- THEN the request is rejected and no loan is created

## REQ-007 - Show active loans

The system SHALL show active loans with the student group, drone, battery quantity, propeller quantity and loan date.

### Scenario: View active loans

- GIVEN at least one active loan exists
- WHEN staff view active loans
- THEN each active loan is shown with its group, drone, accessory quantities and loan date

## REQ-008 - Return a loan

Staff SHALL be able to return an active loan. The loan SHALL change from `Active` to `Returned`, and its drone SHALL become `Available` again.

### Scenario: Return an active loan

- GIVEN "TELLO-07" is loaned to "Group 3"
- WHEN staff return the loan
- THEN the loan is marked `Returned` and "TELLO-07" becomes `Available`

## REQ-009 - Save the return date

The system SHALL record the server-set date when a loan is returned.

### Scenario: Record a return date

- GIVEN an active loan exists
- WHEN staff return the loan
- THEN the loan stores the current date as its return date

## REQ-010 - Preserve loan history

The system SHALL retain returned loans so staff can view the complete loan history.

### Scenario: View a returned loan in history

- GIVEN a loan has been returned
- WHEN staff view loan history
- THEN the loan remains listed with its drone, group, accessory quantities, loan date, returned state and return date
