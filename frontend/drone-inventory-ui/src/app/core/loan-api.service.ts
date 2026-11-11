import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { CreateLoanRequest, Loan, LoanListItem, LoanState } from './api-models';

/**
 * Client for the loan API. Loan dates and the derived state are set by the server; the client only
 * sends the drone, the student group and the optional accessory quantities.
 */
@Injectable({ providedIn: 'root' })
export class LoanApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = '/api/loans';

  /**
   * Lists loans. Without a state the complete history is returned; `Active` shows only loans that
   * are still out and `Returned` only the returned ones.
   */
  list(state?: LoanState): Observable<LoanListItem[]> {
    const params = state ? new HttpParams().set('state', state.toLowerCase()) : undefined;

    return this.http.get<LoanListItem[]>(this.baseUrl, { params });
  }

  /** Creates a loan with the server's current date as the loan date. */
  create(request: CreateLoanRequest): Observable<Loan> {
    return this.http.post<Loan>(this.baseUrl, request);
  }

  /** Returns an active loan; the server stamps the return date and frees the drone. */
  returnLoan(id: number): Observable<Loan> {
    return this.http.post<Loan>(`${this.baseUrl}/${id}/return`, null);
  }
}
