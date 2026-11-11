import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { CreateStudentGroupRequest, StudentGroup } from './api-models';

/** Client for the student group API, used for registration and for the loan form's group picker. */
@Injectable({ providedIn: 'root' })
export class StudentGroupApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = '/api/student-groups';

  /** Lists every student group so it can be selected as a borrower. */
  list(): Observable<StudentGroup[]> {
    return this.http.get<StudentGroup[]>(this.baseUrl);
  }

  /** Creates a student group. */
  create(request: CreateStudentGroupRequest): Observable<StudentGroup> {
    return this.http.post<StudentGroup>(this.baseUrl, request);
  }
}
