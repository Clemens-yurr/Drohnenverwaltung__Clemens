import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { LoanListPage } from './loan-list-page';

describe('LoanListPage', () => {
  let fixture: ComponentFixture<LoanListPage>;
  let httpMock: HttpTestingController;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [LoanListPage],
      providers: [provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();

    httpMock = TestBed.inject(HttpTestingController);
    fixture = TestBed.createComponent(LoanListPage);
  });

  afterEach(() => httpMock.verify());

  const activeLoan = {
    id: 11,
    droneId: 7,
    droneName: 'TELLO-07',
    studentGroupId: 3,
    studentGroupName: 'Group 3',
    batteryQuantity: 2,
    propellerQuantity: 4,
    loanedOn: '2026-11-11',
    state: 'Active',
    returnedOn: null,
  };

  it('lists active loans with group, drone, accessories and loan date', () => {
    fixture.detectChanges();
    httpMock.expectOne('/api/loans?state=active').flush([activeLoan]);
    fixture.detectChanges();

    const row = fixture.nativeElement.querySelector('tbody tr') as HTMLElement;

    expect(row.textContent).toContain('TELLO-07');
    expect(row.textContent).toContain('Group 3');
    expect(row.textContent).toContain('2');
    expect(row.textContent).toContain('4');
    expect(row.textContent).toContain('2026-11-11');
  });

  it('returns a loan and refreshes the active loans', () => {
    fixture.detectChanges();
    httpMock.expectOne('/api/loans?state=active').flush([activeLoan]);
    fixture.detectChanges();

    (fixture.nativeElement.querySelector('tbody button') as HTMLButtonElement).click();

    const returnRequest = httpMock.expectOne({
      method: 'POST',
      url: '/api/loans/11/return',
    });
    expect(returnRequest.request.body).toBeNull();
    returnRequest.flush({ ...activeLoan, state: 'Returned', returnedOn: '2026-11-11' });

    httpMock.expectOne('/api/loans?state=active').flush([]);
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('No active loans.');
  });
});
