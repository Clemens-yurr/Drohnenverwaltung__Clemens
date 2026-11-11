import { Component, inject, signal } from '@angular/core';
import { LoanApiService } from '../../core/loan-api.service';
import { LoanListItem } from '../../core/models';
import { describeHttpError } from '../../core/problem-details';

@Component({
  selector: 'app-loan-list-page',
  templateUrl: './loan-list-page.html',
})
export class LoanListPage {
  private readonly loanApi = inject(LoanApiService);

  protected readonly loans = signal<LoanListItem[]>([]);
  protected readonly errorMessage = signal<string | null>(null);
  protected readonly returningLoanId = signal<number | null>(null);

  constructor() {
    this.load();
  }

  protected load(): void {
    this.loanApi.list('active').subscribe({
      next: (loans) => this.loans.set(loans),
      error: (error) => this.errorMessage.set(describeHttpError(error)),
    });
  }

  protected returnLoan(loan: LoanListItem): void {
    this.errorMessage.set(null);
    this.returningLoanId.set(loan.id);

    this.loanApi.return(loan.id).subscribe({
      next: () => {
        this.returningLoanId.set(null);
        this.load();
      },
      error: (error) => {
        this.returningLoanId.set(null);
        this.errorMessage.set(describeHttpError(error));
      },
    });
  }
}
