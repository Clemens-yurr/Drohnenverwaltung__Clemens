import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { CreateDroneRequest, Drone, DroneStatus } from './models';

@Injectable({ providedIn: 'root' })
export class DroneApiService {
  private readonly http = inject(HttpClient);
  private readonly url = '/api/drones';

  list(status?: DroneStatus): Observable<Drone[]> {
    const params = status ? { status: status.toLowerCase() } : undefined;

    return this.http.get<Drone[]>(this.url, params ? { params } : {});
  }

  create(request: CreateDroneRequest): Observable<Drone> {
    return this.http.post<Drone>(this.url, request);
  }
}
