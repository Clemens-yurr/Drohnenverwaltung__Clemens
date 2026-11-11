import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { StudentGroup } from './models';
import { StudentGroupApiService } from './student-group-api.service';

describe('StudentGroupApiService', () => {
  let service: StudentGroupApiService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });

    service = TestBed.inject(StudentGroupApiService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  it('lists student groups with a GET request', () => {
    let groups: StudentGroup[] | undefined;

    service.list().subscribe((response) => (groups = response));

    const request = httpMock.expectOne('/api/student-groups');
    expect(request.request.method).toBe('GET');
    request.flush([{ id: 1, name: 'Group 3' }]);

    expect(groups).toEqual([{ id: 1, name: 'Group 3' }]);
  });

  it('posts a new student group and returns its identifier and name', () => {
    let created: StudentGroup | undefined;

    service.create({ name: 'Group 10A' }).subscribe((group) => (created = group));

    const request = httpMock.expectOne({ method: 'POST', url: '/api/student-groups' });
    expect(request.request.body).toEqual({ name: 'Group 10A' });

    request.flush({ id: 2, name: 'Group 10A' });
    expect(created).toEqual({ id: 2, name: 'Group 10A' });
  });
});
