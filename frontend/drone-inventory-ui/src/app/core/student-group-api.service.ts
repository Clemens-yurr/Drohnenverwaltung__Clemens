import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { CreateStudentGroupRequest, StudentGroup } from './models';

@Injectable({ providedIn: 'root' })
export class StudentGroupApiService {
  private readonly http = inject(HttpClient);
  private readonly url = '/api/student-groups';

  list(): Observable<StudentGroup[]> {
    return this.http.get<StudentGroup[]>(this.url);
  }

  create(request: CreateStudentGroupRequest): Observable<StudentGroup> {
    return this.http.post<StudentGroup>(this.url, request);
  }
}
