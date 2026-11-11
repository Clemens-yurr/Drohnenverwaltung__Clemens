import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { DroneApiService } from '../../core/drone-api.service';
import { Drone } from '../../core/models';
import { describeHttpError } from '../../core/problem-details';

@Component({
  selector: 'app-drone-page',
  imports: [ReactiveFormsModule],
  templateUrl: './drone-page.html',
})
export class DronePage {
  private readonly droneApi = inject(DroneApiService);

  protected readonly drones = signal<Drone[]>([]);
  protected readonly errorMessage = signal<string | null>(null);
  protected readonly successMessage = signal<string | null>(null);

  protected readonly form = inject(FormBuilder).nonNullable.group({
    name: ['', Validators.required],
    description: ['', Validators.required],
  });

  constructor() {
    this.load();
  }

  protected load(): void {
    this.droneApi.list().subscribe({
      next: (drones) => this.drones.set(drones),
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

    this.droneApi.create(this.form.getRawValue()).subscribe({
      next: (drone) => {
        this.successMessage.set(`Drone "${drone.name}" was registered.`);
        this.form.reset({ name: '', description: '' });
        this.load();
      },
      error: (error) => this.errorMessage.set(describeHttpError(error)),
    });
  }
}
