import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { LoanApiService } from './loan-api.service';
import { LoanListItem } from './models';

describe('LoanApiService', () => {
  let service: LoanApiService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });

    service = TestBed.inject(LoanApiService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  it('lists active loans with the active state filter', () => {
    let loans: LoanListItem[] | undefined;

    service.list('active').subscribe((response) => (loans = response));

    const request = httpMock.expectOne('/api/loans?state=active');
    expect(request.request.method).toBe('GET');

    request.flush([
      {
        id: 1,
        droneId: 7,
        droneName: 'TELLO-07',
        studentGroupId: 3,
        studentGroupName: 'Group 3',
        batteryQuantity: 2,
        propellerQuantity: 4,
        loanedOn: '2026-11-11',
        state: 'Active',
        returnedOn: null,
      },
    ]);

    expect(loans?.[0].state).toBe('Active');
  });

  it('lists the complete loan history without a state filter', () => {
    service.list().subscribe();

    const request = httpMock.expectOne('/api/loans');
    expect(request.request.method).toBe('GET');
    request.flush([]);
  });

  it('creates a loan with the accessory types as enum strings', () => {
    service
      .create({
        droneId: 7,
        studentGroupId: 3,
        items: [
          { type: 'Battery', quantity: 2 },
          { type: 'Propeller', quantity: 4 },
        ],
      })
      .subscribe();

    const request = httpMock.expectOne({ method: 'POST', url: '/api/loans' });
    expect(request.request.body).toEqual({
      droneId: 7,
      studentGroupId: 3,
      items: [
        { type: 'Battery', quantity: 2 },
        { type: 'Propeller', quantity: 4 },
      ],
    });

    request.flush({
      id: 11,
      droneId: 7,
      droneName: 'TELLO-07',
      studentGroupId: 3,
      studentGroupName: 'Group 3',
      loanedOn: '2026-11-11',
      returnedOn: null,
    });
  });

  it('returns a loan with a POST to the return endpoint', () => {
    service.return(11).subscribe();

    const request = httpMock.expectOne({ method: 'POST', url: '/api/loans/11/return' });
    expect(request.request.body).toBeNull();

    request.flush({
      id: 11,
      droneId: 7,
      droneName: 'TELLO-07',
      studentGroupId: 3,
      studentGroupName: 'Group 3',
      loanedOn: '2026-11-11',
      returnedOn: '2026-11-11',
    });
  });
});
