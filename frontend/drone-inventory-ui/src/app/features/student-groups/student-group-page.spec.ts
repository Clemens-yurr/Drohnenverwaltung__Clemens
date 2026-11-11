import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { setInputValue, submitForm } from '../../testing/forms';
import { StudentGroupPage } from './student-group-page';

describe('StudentGroupPage', () => {
  let fixture: ComponentFixture<StudentGroupPage>;
  let httpMock: HttpTestingController;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [StudentGroupPage],
      providers: [provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();

    httpMock = TestBed.inject(HttpTestingController);
    fixture = TestBed.createComponent(StudentGroupPage);
  });

  afterEach(() => httpMock.verify());

  it('lists the registered student groups', () => {
    fixture.detectChanges();
    httpMock.expectOne('/api/student-groups').flush([
      { id: 1, name: 'Group 3' },
      { id: 2, name: 'Group 10A' },
    ]);
    fixture.detectChanges();

    const rows = Array.from(fixture.nativeElement.querySelectorAll('tbody tr')) as HTMLElement[];

    expect(rows).toHaveLength(2);
    expect(rows[0].textContent).toContain('Group 3');
    expect(rows[1].textContent).toContain('Group 10A');
  });

  it('creates a student group and reloads the list', () => {
    fixture.detectChanges();
    httpMock.expectOne('/api/student-groups').flush([]);

    setInputValue(fixture, 'input[formControlName="name"]', 'Group 10B');
    submitForm(fixture);

    const create = httpMock.expectOne({ method: 'POST', url: '/api/student-groups' });
    expect(create.request.body).toEqual({ name: 'Group 10B' });
    create.flush({ id: 3, name: 'Group 10B' });

    httpMock.expectOne('/api/student-groups').flush([{ id: 3, name: 'Group 10B' }]);
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('Group 10B');
    expect(fixture.nativeElement.textContent).toContain('was created');
  });

  it('shows a validation message and does not post an empty name', () => {
    fixture.detectChanges();
    httpMock.expectOne('/api/student-groups').flush([]);

    submitForm(fixture);

    expect(fixture.nativeElement.textContent).toContain('A student group name is required.');
    httpMock.expectNone({ method: 'POST' });
  });
});
