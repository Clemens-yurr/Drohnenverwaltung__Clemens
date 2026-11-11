import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { LoanHistoryPage } from './loan-history-page';

describe('LoanHistoryPage', () => {
  let fixture: ComponentFixture<LoanHistoryPage>;
  let httpMock: HttpTestingController;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [LoanHistoryPage],
      providers: [provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();

    httpMock = TestBed.inject(HttpTestingController);
    fixture = TestBed.createComponent(LoanHistoryPage);
  });

  afterEach(() => httpMock.verify());

  it('shows active and returned loans with state, accessories and dates', () => {
    fixture.detectChanges();
    httpMock.expectOne('/api/loans').flush([
      {
        id: 11,
        droneId: 7,
        droneName: 'TELLO-07',
        studentGroupId: 3,
        studentGroupName: 'Group 3',
        batteryQuantity: 2,
        propellerQuantity: 4,
        loanedOn: '2026-11-10',
        state: 'Active',
        returnedOn: null,
      },
      {
        id: 12,
        droneId: 8,
        droneName: 'TELLO-08',
        studentGroupId: 4,
        studentGroupName: 'Group 4',
        batteryQuantity: 1,
        propellerQuantity: 0,
        loanedOn: '2026-11-01',
        state: 'Returned',
        returnedOn: '2026-11-05',
      },
    ]);
    fixture.detectChanges();

    const rows = Array.from(fixture.nativeElement.querySelectorAll('tbody tr')) as HTMLElement[];

    expect(rows).toHaveLength(2);
    expect(rows[0].textContent).toContain('TELLO-07');
    expect(rows[0].textContent).toContain('Group 3');
    expect(rows[0].textContent).toContain('2026-11-10');
    expect(rows[0].textContent).toContain('Active');
    expect(rows[0].textContent).toContain('not returned');

    expect(rows[1].textContent).toContain('TELLO-08');
    expect(rows[1].textContent).toContain('Group 4');
    expect(rows[1].textContent).toContain('Returned');
    expect(rows[1].textContent).toContain('2026-11-05');
  });
});
