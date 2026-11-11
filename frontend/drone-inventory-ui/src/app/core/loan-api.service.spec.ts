import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { Loan, LoanListItem } from './api-models';
import { LoanApiService } from './loan-api.service';

describe('LoanApiService', () => {
  let service: LoanApiService;
  let httpTesting: HttpTestingController;

  const activeLoan: LoanListItem = {
    id: 1,
    droneId: 1,
    droneName: 'TELLO-07',
    studentGroupId: 1,
    studentGroupName: 'Group 3',
    batteryQuantity: 2,
    propellerQuantity: 4,
    loanedOn: '2026-11-04',
    state: 'Active',
    returnedOn: null,
  };

  const returnedLoan: LoanListItem = {
    ...activeLoan,
    id: 2,
    state: 'Returned',
    returnedOn: '2026-11-05',
  };

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });

    service = TestBed.inject(LoanApiService);
    httpTesting = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpTesting.verify());

  it('loads the complete history through GET /api/loans without a state filter', () => {
    let result: LoanListItem[] | undefined;

    service.list().subscribe((loans) => (result = loans));

    const request = httpTesting.expectOne('/api/loans');
    expect(request.request.method).toBe('GET');
    expect(request.request.params.has('state')).toBe(false);
    request.flush([activeLoan, returnedLoan]);

    expect(result).toEqual([activeLoan, returnedLoan]);
    expect(result?.[1].state).toBe('Returned');
  });

  it('requests active loans with the lowercase state value', () => {
    service.list('Active').subscribe();

    const request = httpTesting.expectOne('/api/loans?state=active');
    expect(request.request.method).toBe('GET');
    expect(request.request.params.get('state')).toBe('active');
    request.flush([activeLoan]);
  });

  it('requests returned loans with the lowercase state value', () => {
    service.list('Returned').subscribe();

    const request = httpTesting.expectOne('/api/loans?state=returned');
    expect(request.request.params.get('state')).toBe('returned');
    request.flush([returnedLoan]);
  });

  it('creates a loan through POST /api/loans with accessory enum strings', () => {
    const created: Loan = {
      id: 7,
      droneId: 1,
      droneName: 'TELLO-07',
      studentGroupId: 1,
      studentGroupName: 'Group 3',
      loanedOn: '2026-11-11',
      returnedOn: null,
    };
    let result: Loan | undefined;

    service
      .create({
        droneId: 1,
        studentGroupId: 1,
        items: [
          { type: 'Battery', quantity: 2 },
          { type: 'Propeller', quantity: 4 },
        ],
      })
      .subscribe((loan) => (result = loan));

    const request = httpTesting.expectOne('/api/loans');
    expect(request.request.method).toBe('POST');
    expect(request.request.body).toEqual({
      droneId: 1,
      studentGroupId: 1,
      items: [
        { type: 'Battery', quantity: 2 },
        { type: 'Propeller', quantity: 4 },
      ],
    });
    request.flush(created, { status: 201, statusText: 'Created' });

    expect(result).toEqual(created);
  });

  it('omits the accessory list when a loan has no accessories', () => {
    service.create({ droneId: 1, studentGroupId: 1 }).subscribe();

    const request = httpTesting.expectOne('/api/loans');
    expect(request.request.method).toBe('POST');
    expect(request.request.body).toEqual({ droneId: 1, studentGroupId: 1 });
    request.flush({
      id: 8,
      droneId: 1,
      droneName: 'TELLO-07',
      studentGroupId: 1,
      studentGroupName: 'Group 3',
      loanedOn: '2026-11-11',
      returnedOn: null,
    });
  });

  it('returns a loan through POST /api/loans/{id}/return without a request body', () => {
    const returned: Loan = {
      id: 7,
      droneId: 1,
      droneName: 'TELLO-07',
      studentGroupId: 1,
      studentGroupName: 'Group 3',
      loanedOn: '2026-11-04',
      returnedOn: '2026-11-11',
    };
    let result: Loan | undefined;

    service.returnLoan(7).subscribe((loan) => (result = loan));

    const request = httpTesting.expectOne('/api/loans/7/return');
    expect(request.request.method).toBe('POST');
    expect(request.request.body).toBeNull();
    request.flush(returned);

    expect(result).toEqual(returned);
    expect(result?.returnedOn).toBe('2026-11-11');
  });
});
