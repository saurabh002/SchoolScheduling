import { Component, inject, signal } from '@angular/core';
import { PendingSubstitutionDto } from '../model/absence.model';
import { AbsenceService } from '../services/absence.service';


@Component({
  selector: 'app-substitutions',
  imports: [],
  templateUrl: './substitutions.html',
  styleUrl: './substitutions.css',
})
export class Substitutions {
  private absenceService = inject(AbsenceService);

  selectedDate = signal<string>(new Date().toISOString().split('T')[0]);
  pendingSubstitutions = signal<PendingSubstitutionDto[]>([]);
  isLoadingPending = signal<boolean>(false);
  pendingError = signal<string | null>(null);

  getPendingSubstitutions(): void {
    this.isLoadingPending.set(true);
    this.pendingError.set(null);
    console.log('Fetching pending substitutions for date:', this.selectedDate());
    this.absenceService.getPendingSubstitutions(this.selectedDate()).subscribe({
      next: (data) => {
        this.pendingSubstitutions.set(data);
        this.isLoadingPending.set(false);
        console.log('Pending substitutions loaded:', data);
      },
      error: () => {
        this.pendingError.set('Failed to load pending substitutions.');
        this.isLoadingPending.set(false);
      },
    });
  }

  constructor() {
    this.getPendingSubstitutions();
  }
}
