import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { CreateLoanRequest, Loan, LoanListItem } from './models';

export type LoanStateFilter = 'active' | 'returned';

@Injectable({ providedIn: 'root' })
export class LoanApiService {
  private readonly http = inject(HttpClient);
  private readonly url = '/api/loans';

  list(state?: LoanStateFilter): Observable<LoanListItem[]> {
    return this.http.get<LoanListItem[]>(this.url, state ? { params: { state } } : {});
  }

  create(request: CreateLoanRequest): Observable<Loan> {
    return this.http.post<Loan>(this.url, request);
  }

  return(loanId: number): Observable<Loan> {
    return this.http.post<Loan>(`${this.url}/${loanId}/return`, null);
  }
}
