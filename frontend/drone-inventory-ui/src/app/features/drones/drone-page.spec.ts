import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { setInputValue, submitForm } from '../../testing/forms';
import { DronePage } from './drone-page';

describe('DronePage', () => {
  let fixture: ComponentFixture<DronePage>;
  let httpMock: HttpTestingController;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DronePage],
      providers: [provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();

    httpMock = TestBed.inject(HttpTestingController);
    fixture = TestBed.createComponent(DronePage);
  });

  afterEach(() => httpMock.verify());

  it('lists drones with name, description and derived status', () => {
    fixture.detectChanges();
    httpMock.expectOne('/api/drones').flush([
      { id: 1, name: 'TELLO-07', description: 'Training drone', status: 'Available' },
      { id: 2, name: 'TELLO-08', description: 'Out on loan', status: 'Loaned' },
    ]);
    fixture.detectChanges();

    const rows = Array.from(fixture.nativeElement.querySelectorAll('tbody tr')) as HTMLElement[];

    expect(rows).toHaveLength(2);
    expect(rows[0].textContent).toContain('TELLO-07');
    expect(rows[0].textContent).toContain('Training drone');
    expect(rows[0].textContent).toContain('Available');
    expect(rows[1].textContent).toContain('TELLO-08');
    expect(rows[1].textContent).toContain('Loaned');
  });

  it('posts a new drone and reloads the list', () => {
    fixture.detectChanges();
    httpMock.expectOne('/api/drones').flush([]);

    setInputValue(fixture, 'input[formControlName="name"]', 'TELLO-09');
    setInputValue(fixture, 'input[formControlName="description"]', 'New training drone');
    submitForm(fixture);

    const create = httpMock.expectOne({ method: 'POST', url: '/api/drones' });
    expect(create.request.body).toEqual({
      name: 'TELLO-09',
      description: 'New training drone',
    });
    create.flush({
      id: 3,
      name: 'TELLO-09',
      description: 'New training drone',
      status: 'Available',
    });

    httpMock.expectOne('/api/drones').flush([
      { id: 3, name: 'TELLO-09', description: 'New training drone', status: 'Available' },
    ]);
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('TELLO-09');
    expect(fixture.nativeElement.textContent).toContain('was registered');
  });

  it('shows validation messages and does not post an invalid form', () => {
    fixture.detectChanges();
    httpMock.expectOne('/api/drones').flush([]);

    submitForm(fixture);

    expect(fixture.nativeElement.textContent).toContain('A drone name is required.');
    expect(fixture.nativeElement.textContent).toContain('A drone description is required.');
    httpMock.expectNone({ method: 'POST' });
  });
});
