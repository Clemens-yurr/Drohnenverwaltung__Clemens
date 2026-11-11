import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { CreateDroneRequest, Drone } from './api-models';

/** Client for the drone API. Availability is derived by the server from active loans. */
@Injectable({ providedIn: 'root' })
export class DroneApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = '/api/drones';

  /** Lists every drone with its name, description and derived status. */
  list(): Observable<Drone[]> {
    return this.http.get<Drone[]>(this.baseUrl);
  }

  /** Creates a drone; the server answers with the stored drone as `Available`. */
  create(request: CreateDroneRequest): Observable<Drone> {
    return this.http.post<Drone>(this.baseUrl, request);
  }
}
