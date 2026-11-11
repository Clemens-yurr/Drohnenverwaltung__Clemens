import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { DroneApiService } from '../../core/drone-api.service';
import { LoanApiService } from '../../core/loan-api.service';
import { CreateLoanRequest, Drone, StudentGroup } from '../../core/models';
import { describeHttpError } from '../../core/problem-details';
import { StudentGroupApiService } from '../../core/student-group-api.service';

@Component({
  selector: 'app-loan-form-page',
  imports: [ReactiveFormsModule],
  templateUrl: './loan-form-page.html',
})
export class LoanFormPage {
  private readonly droneApi = inject(DroneApiService);
  private readonly studentGroupApi = inject(StudentGroupApiService);
  private readonly loanApi = inject(LoanApiService);

  protected readonly drones = signal<Drone[]>([]);
  protected readonly groups = signal<StudentGroup[]>([]);
  protected readonly errorMessage = signal<string | null>(null);
  protected readonly successMessage = signal<string | null>(null);

  protected readonly quantityValidators = [
    Validators.required,
    Validators.min(0),
    Validators.pattern(/^\d+$/),
  ];

  protected readonly form = inject(FormBuilder).group({
    droneId: [null as number | null, Validators.required],
    studentGroupId: [null as number | null, Validators.required],
    batteryQuantity: [0, this.quantityValidators],
    propellerQuantity: [0, this.quantityValidators],
  });

  constructor() {
    this.loadOptions();
  }

  protected loadOptions(): void {
    this.droneApi.list('Available').subscribe({
      next: (drones) => this.drones.set(drones),
      error: (error) => this.errorMessage.set(describeHttpError(error)),
    });

    this.studentGroupApi.list().subscribe({
      next: (groups) => this.groups.set(groups),
      error: (error) => this.errorMessage.set(describeHttpError(error)),
    });
  }

  protected submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const value = this.form.getRawValue();
    const request: CreateLoanRequest = {
      droneId: value.droneId!,
      studentGroupId: value.studentGroupId!,
      items: [
        { type: 'Battery', quantity: value.batteryQuantity! },
        { type: 'Propeller', quantity: value.propellerQuantity! },
      ],
    };

    this.errorMessage.set(null);
    this.successMessage.set(null);

    this.loanApi.create(request).subscribe({
      next: (loan) => {
        this.successMessage.set(
          `"${loan.droneName}" is now loaned to ${loan.studentGroupName}.`,
        );
        this.form.reset({
          droneId: null,
          studentGroupId: null,
          batteryQuantity: 0,
          propellerQuantity: 0,
        });
        this.loadOptions();
      },
      error: (error) => this.errorMessage.set(describeHttpError(error)),
    });
  }
}
