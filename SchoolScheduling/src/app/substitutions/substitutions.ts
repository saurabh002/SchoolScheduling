import { Component, computed, inject, signal } from '@angular/core';
import { AssignedSubstitutionDto, PendingSubstitutionDto } from '../model/absence.model';
import { AbsenceService } from '../services/absence.service';
import { rxResource } from '@angular/core/rxjs-interop';
import { firstValueFrom } from 'rxjs';
import { FormsModule } from '@angular/forms';
import { SubstituteDropdown } from './substitute-dropdown/substitute-dropdown';


@Component({
  selector: 'app-substitutions',
  imports: [FormsModule, SubstituteDropdown],
  templateUrl: './substitutions.html',
  styleUrl: './substitutions.css',
})
export class Substitutions {
  private absenceService = inject(AbsenceService);

  private getTodayLocalDate(): string {
    const now = new Date();
    const year = now.getFullYear();
    const month = String(now.getMonth() + 1).padStart(2, '0');
    const day = String(now.getDate()).padStart(2, '0');
    return `${year}-${month}-${day}`;
  }

  selectedDate = signal<string>(this.getTodayLocalDate());
  pendingSubstitutions = signal<PendingSubstitutionDto[]>([]);
  isLoadingPending = signal<boolean>(false);
  pendingError = signal<string | null>(null);
  selectedSubstitutes = signal<Record<number, number>>({});
  assigning = signal<number | null>(null);
  activeTab = signal<'pending' | 'assigned'>('pending');
  errorMessage = signal<string | null>(null);
  unassigning = signal<number | null>(null);

  pending = rxResource({
    params: () => ({ date: this.selectedDate() }),
    stream: ({params}) => this.absenceService.getPendingSubstitutions(params.date)
  });

  assigned = rxResource<AssignedSubstitutionDto[], { date: string }>({
    params: () => ({ date: this.selectedDate() }),
    stream: ({params}) => this.absenceService.getAssignedSubstitutions(params.date),
  });

  pendingCount = computed(() => this.pending.value()?.length ?? 0);
  assignedCount = computed(() => this.assigned.value()?.length ?? 0);


  selectSubstitute(absencePeriodId: number, substituteTeacherId: number): void {
    console.log(`Selected substitute for absencePeriodId ${absencePeriodId}: ${substituteTeacherId}`);
    this.selectedSubstitutes.update((prev) => ({
      ...prev,
      [absencePeriodId]: substituteTeacherId,
    }));  
  }

  async assign(periodId: number) {
    console.log(`Assigning substitute for periodId ${periodId}`);
    console.log(` substitutes: ${this.selectedSubstitutes()[periodId]}`);
    const selectedSub = this.selectedSubstitutes()[periodId];
    if (selectedSub) {
      this.assigning.set(periodId);
      try{
         await firstValueFrom(this.absenceService.assignSubstitute(periodId, selectedSub));
         this.pending.reload();
         this.assigned.reload();
         this.selectedSubstitutes.update((prev) => {
            const updated = { ...prev };
            delete updated[periodId];
            return updated;
          });
         }
      
      catch (error) {
        console.error('Error assigning substitute:', error);
        this.errorMessage.set('Failed to assign substitute. Please try again.');
        this.assigning.set(null)
      }
      finally {
        this.assigning.set(null);
      }
    }
  }

  async unassign(absencePeriodId: number) {
    this.unassigning.set(absencePeriodId);
    this.errorMessage.set(null);
    try {
      await firstValueFrom(this.absenceService.unassign(absencePeriodId));
      this.pending.reload();
      this.assigned.reload();
    } catch (err: any) {
      this.errorMessage.set(err.error?.message ?? 'Failed to cancel assignment.');
    } finally {
      this.unassigning.set(null);
    }
  }
}
