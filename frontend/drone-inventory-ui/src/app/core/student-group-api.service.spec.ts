import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { StudentGroup } from './api-models';
import { StudentGroupApiService } from './student-group-api.service';

describe('StudentGroupApiService', () => {
  let service: StudentGroupApiService;
  let httpTesting: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });

    service = TestBed.inject(StudentGroupApiService);
    httpTesting = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpTesting.verify());

  it('lists student groups through GET /api/student-groups', () => {
    const groups: StudentGroup[] = [
      { id: 1, name: 'Group 3' },
      { id: 2, name: 'Group 4' },
    ];
    let result: StudentGroup[] | undefined;

    service.list().subscribe((groups) => (result = groups));

    const request = httpTesting.expectOne('/api/student-groups');
    expect(request.request.method).toBe('GET');
    request.flush(groups);

    expect(result).toEqual(groups);
  });

  it('creates a student group through POST /api/student-groups with the name only', () => {
    const created: StudentGroup = { id: 3, name: 'Group 5' };
    let result: StudentGroup | undefined;

    service.create({ name: 'Group 5' }).subscribe((group) => (result = group));

    const request = httpTesting.expectOne('/api/student-groups');
    expect(request.request.method).toBe('POST');
    expect(request.request.body).toEqual({ name: 'Group 5' });
    request.flush(created, { status: 201, statusText: 'Created' });

    expect(result).toEqual(created);
  });
});
