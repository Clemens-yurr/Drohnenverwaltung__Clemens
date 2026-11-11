/**
 * A drone status as sent by the API. It is derived from the drone's active loans, so the client
 * never sets it.
 */
export type DroneStatus = 'Available' | 'Loaned';

/** A drone as returned by the API, including its derived status. */
export interface Drone {
  readonly id: number;
  readonly name: string;
  readonly description: string;
  readonly status: DroneStatus;
}

/** The payload for creating a drone. The status is derived, so it is not part of it. */
export interface CreateDroneRequest {
  readonly name: string;
  readonly description: string;
}

/** A student group as returned by the API and selectable in the loan form. */
export interface StudentGroup {
  readonly id: number;
  readonly name: string;
}

/** The payload for creating a student group. */
export interface CreateStudentGroupRequest {
  readonly name: string;
}

/** An accessory type a loan records a quantity for. */
export type LoanItemType = 'Battery' | 'Propeller';

/** The state of a loan, derived from its return date. */
export type LoanState = 'Active' | 'Returned';

/** One accessory position of a loan: a battery or propeller quantity of at least 0. */
export interface CreateLoanItemRequest {
  readonly type: LoanItemType;
  readonly quantity: number;
}

/** The payload for creating a loan. The loan date is set by the server. */
export interface CreateLoanRequest {
  readonly droneId: number;
  readonly studentGroupId: number;
  readonly items?: readonly CreateLoanItemRequest[];
}

/**
 * A loan as returned after creation and on return. The return date is null while the loan is
 * active.
 */
export interface Loan {
  readonly id: number;
  readonly droneId: number;
  readonly droneName: string;
  readonly studentGroupId: number;
  readonly studentGroupName: string;
  /** Server-set loan date in `yyyy-MM-dd` form. */
  readonly loanedOn: string;
  /** Server-set return date in `yyyy-MM-dd` form, or null while the loan is active. */
  readonly returnedOn: string | null;
}

/** A loan in the active list or the loan history, with accessory quantities and state. */
export interface LoanListItem {
  readonly id: number;
  readonly droneId: number;
  readonly droneName: string;
  readonly studentGroupId: number;
  readonly studentGroupName: string;
  readonly batteryQuantity: number;
  readonly propellerQuantity: number;
  readonly loanedOn: string;
  readonly state: LoanState;
  readonly returnedOn: string | null;
}
