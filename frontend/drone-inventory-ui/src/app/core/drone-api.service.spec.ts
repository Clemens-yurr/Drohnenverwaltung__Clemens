import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { Drone } from './api-models';
import { DroneApiService } from './drone-api.service';

describe('DroneApiService', () => {
  let service: DroneApiService;
  let httpTesting: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });

    service = TestBed.inject(DroneApiService);
    httpTesting = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpTesting.verify());

  it('lists drones through GET /api/drones and keeps the status strings', () => {
    const drones: Drone[] = [
      { id: 1, name: 'TELLO-07', description: 'Ready to fly', status: 'Available' },
      { id: 2, name: 'TELLO-08', description: 'Out with a group', status: 'Loaned' },
    ];
    let result: Drone[] | undefined;

    service.list().subscribe((drones) => (result = drones));

    const request = httpTesting.expectOne('/api/drones');
    expect(request.request.method).toBe('GET');
    expect(request.request.body).toBeNull();
    request.flush(drones);

    expect(result).toEqual(drones);
    expect(result?.[0].status).toBe('Available');
    expect(result?.[1].status).toBe('Loaned');
  });

  it('creates a drone through POST /api/drones with name and description only', () => {
    const created: Drone = {
      id: 3,
      name: 'TELLO-09',
      description: 'New drone',
      status: 'Available',
    };
    let result: Drone | undefined;

    service
      .create({ name: 'TELLO-09', description: 'New drone' })
      .subscribe((drone) => (result = drone));

    const request = httpTesting.expectOne('/api/drones');
    expect(request.request.method).toBe('POST');
    expect(request.request.body).toEqual({ name: 'TELLO-09', description: 'New drone' });
    request.flush(created, { status: 201, statusText: 'Created' });

    expect(result).toEqual(created);
  });
});
