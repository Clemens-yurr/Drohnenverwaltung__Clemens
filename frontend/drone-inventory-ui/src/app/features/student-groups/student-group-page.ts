import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { StudentGroupApiService } from '../../core/student-group-api.service';
import { StudentGroup } from '../../core/models';
import { describeHttpError } from '../../core/problem-details';

@Component({
  selector: 'app-student-group-page',
  imports: [ReactiveFormsModule],
  templateUrl: './student-group-page.html',
})
export class StudentGroupPage {
  private readonly studentGroupApi = inject(StudentGroupApiService);

  protected readonly groups = signal<StudentGroup[]>([]);
  protected readonly errorMessage = signal<string | null>(null);
  protected readonly successMessage = signal<string | null>(null);

  protected readonly form = inject(FormBuilder).nonNullable.group({
    name: ['', Validators.required],
  });

  constructor() {
    this.load();
  }

  protected load(): void {
    this.studentGroupApi.list().subscribe({
      next: (groups) => this.groups.set(groups),
      error: (error) => this.errorMessage.set(describeHttpError(error)),
    });
  }

  protected create(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.errorMessage.set(null);
    this.successMessage.set(null);

    this.studentGroupApi.create(this.form.getRawValue()).subscribe({
      next: (group) => {
        this.successMessage.set(`Student group "${group.name}" was created.`);
        this.form.reset({ name: '' });
        this.load();
      },
      error: (error) => this.errorMessage.set(describeHttpError(error)),
    });
  }
}
