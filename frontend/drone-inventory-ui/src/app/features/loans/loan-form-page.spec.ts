import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { selectOption, setInputValue, submitForm } from '../../testing/forms';
import { LoanFormPage } from './loan-form-page';

describe('LoanFormPage', () => {
  let fixture: ComponentFixture<LoanFormPage>;
  let httpMock: HttpTestingController;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [LoanFormPage],
      providers: [provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();

    httpMock = TestBed.inject(HttpTestingController);
    fixture = TestBed.createComponent(LoanFormPage);
  });

  afterEach(() => httpMock.verify());

  function flushOptions(): void {
    httpMock.expectOne('/api/drones?status=available').flush([
      { id: 7, name: 'TELLO-07', description: 'Training drone', status: 'Available' },
    ]);
    httpMock.expectOne('/api/student-groups').flush([{ id: 3, name: 'Group 3' }]);
    fixture.detectChanges();
  }

  it('offers available drones and existing student groups', () => {
    fixture.detectChanges();
    flushOptions();

    const droneOptions = fixture.nativeElement.querySelectorAll('#loan-drone option');
    const groupOptions = fixture.nativeElement.querySelectorAll('#loan-group option');

    expect(droneOptions).toHaveLength(2);
    expect(droneOptions[1].textContent).toContain('TELLO-07');
    expect(groupOptions).toHaveLength(2);
    expect(groupOptions[1].textContent).toContain('Group 3');
  });

  it('requires a drone and a student group before submitting', () => {
    fixture.detectChanges();
    flushOptions();

    submitForm(fixture);

    expect(fixture.nativeElement.textContent).toContain('Choose an available drone.');
    expect(fixture.nativeElement.textContent).toContain('Choose a student group.');
    httpMock.expectNone({ method: 'POST' });
  });

  it('rejects a negative accessory quantity', () => {
    fixture.detectChanges();
    flushOptions();

    selectOption(fixture, '#loan-drone', 1);
    selectOption(fixture, '#loan-group', 1);
    setInputValue(fixture, '#loan-batteries', '-1');
    submitForm(fixture);

    expect(fixture.nativeElement.textContent).toContain('Enter a whole number of 0 or more.');
    httpMock.expectNone({ method: 'POST' });
  });

  it('submits the selected drone, group and accessory quantities', () => {
    fixture.detectChanges();
    flushOptions();

    selectOption(fixture, '#loan-drone', 1);
    selectOption(fixture, '#loan-group', 1);
    setInputValue(fixture, '#loan-batteries', '2');
    setInputValue(fixture, '#loan-propellers', '4');
    submitForm(fixture);

    const create = httpMock.expectOne({ method: 'POST', url: '/api/loans' });
    expect(create.request.body).toEqual({
      droneId: 7,
      studentGroupId: 3,
      items: [
        { type: 'Battery', quantity: 2 },
        { type: 'Propeller', quantity: 4 },
      ],
    });

    create.flush({
      id: 11,
      droneId: 7,
      droneName: 'TELLO-07',
      studentGroupId: 3,
      studentGroupName: 'Group 3',
      loanedOn: '2026-11-11',
      returnedOn: null,
    });
    flushOptions();

    expect(fixture.nativeElement.textContent).toContain('is now loaned to Group 3');
  });

  it('shows the API conflict message for a drone that is already loaned', () => {
    fixture.detectChanges();
    flushOptions();

    selectOption(fixture, '#loan-drone', 1);
    selectOption(fixture, '#loan-group', 1);
    submitForm(fixture);

    const create = httpMock.expectOne({ method: 'POST', url: '/api/loans' });
    create.flush(
      {
        type: 'about:blank',
        title: 'Drone not available',
        status: 409,
        detail: 'Drone 7 already has an active loan.',
      },
      { status: 409, statusText: 'Conflict' },
    );
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('Drone 7 already has an active loan.');
  });
});
