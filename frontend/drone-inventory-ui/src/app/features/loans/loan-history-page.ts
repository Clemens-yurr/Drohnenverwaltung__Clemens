import { Component, inject, signal } from '@angular/core';
import { LoanApiService } from '../../core/loan-api.service';
import { LoanListItem } from '../../core/models';
import { describeHttpError } from '../../core/problem-details';

@Component({
  selector: 'app-loan-history-page',
  templateUrl: './loan-history-page.html',
})
export class LoanHistoryPage {
  private readonly loanApi = inject(LoanApiService);

  protected readonly loans = signal<LoanListItem[]>([]);
  protected readonly errorMessage = signal<string | null>(null);

  constructor() {
    this.loanApi.list().subscribe({
      next: (loans) => this.loans.set(loans),
      error: (error) => this.errorMessage.set(describeHttpError(error)),
    });
  }
}
