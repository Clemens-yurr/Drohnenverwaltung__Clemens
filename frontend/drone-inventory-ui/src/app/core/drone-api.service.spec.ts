import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { DroneApiService } from './drone-api.service';
import { Drone } from './models';

describe('DroneApiService', () => {
  let service: DroneApiService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });

    service = TestBed.inject(DroneApiService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  it('lists all drones with a GET request', () => {
    let drones: Drone[] | undefined;

    service.list().subscribe((response) => (drones = response));

    const request = httpMock.expectOne('/api/drones');
    expect(request.request.method).toBe('GET');
    request.flush([{ id: 1, name: 'TELLO-07', description: 'Training', status: 'Available' }]);

    expect(drones?.map((drone) => drone.status)).toEqual(['Available']);
  });

  it('asks the API for available drones when a status is given', () => {
    service.list('Available').subscribe();

    const request = httpMock.expectOne('/api/drones?status=available');
    expect(request.request.method).toBe('GET');
    request.flush([]);
  });

  it('posts a new drone and returns it with its status enum string', () => {
    let created: Drone | undefined;

    service
      .create({ name: 'TELLO-08', description: 'Spare drone' })
      .subscribe((drone) => (created = drone));

    const request = httpMock.expectOne({ method: 'POST', url: '/api/drones' });
    expect(request.request.body).toEqual({ name: 'TELLO-08', description: 'Spare drone' });

    request.flush({ id: 2, name: 'TELLO-08', description: 'Spare drone', status: 'Loaned' });
    expect(created).toEqual({
      id: 2,
      name: 'TELLO-08',
      description: 'Spare drone',
      status: 'Loaned',
    });
  });
});
