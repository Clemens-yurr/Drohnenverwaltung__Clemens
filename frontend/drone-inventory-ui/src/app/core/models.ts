/** The status the API derives from a drone's active loan. */
export type DroneStatus = 'Available' | 'Loaned';

/** The state the API derives from a loan's return date. */
export type LoanState = 'Active' | 'Returned';

/** Accessories are tracked only as quantities per type. */
export type LoanItemType = 'Battery' | 'Propeller';

export interface Drone {
  id: number;
  name: string;
  description: string;
  status: DroneStatus;
}

export interface CreateDroneRequest {
  name: string;
  description: string;
}

export interface StudentGroup {
  id: number;
  name: string;
}

export interface CreateStudentGroupRequest {
  name: string;
}

export interface CreateLoanItemRequest {
  type: LoanItemType;
  quantity: number;
}

export interface CreateLoanRequest {
  droneId: number;
  studentGroupId: number;
  items: CreateLoanItemRequest[];
}

/** A loan as returned when it is created or returned. */
export interface Loan {
  id: number;
  droneId: number;
  droneName: string;
  studentGroupId: number;
  studentGroupName: string;
  loanedOn: string;
  returnedOn: string | null;
}

/** A loan in the active list or the complete history, with accessory quantities and state. */
export interface LoanListItem extends Loan {
  batteryQuantity: number;
  propellerQuantity: number;
  state: LoanState;
}
