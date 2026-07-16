import { Component, computed, ElementRef, HostListener, input, OnDestroy, output, signal } from '@angular/core';
import { AvailableSubstituteDto } from '../../model/absence.model';

@Component({
  selector: 'app-substitute-dropdown',
  imports: [],
  templateUrl: './substitute-dropdown.html',
  styleUrl: './substitute-dropdown.css',
})
export class SubstituteDropdown {
substitutes = input.required<AvailableSubstituteDto[]>();
  selected = input<number | null>(null);
  substituteSelected = output<number>();

  isOpen = signal(false);
  
  warningVisible = signal(false);
  private warningTimer?: ReturnType<typeof setTimeout>;

  selectedSub = computed(() =>
    this.substitutes().find(s => s.teacherId === this.selected()) ?? null
  );

  constructor(private elRef: ElementRef) {}

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent) {
    if (!this.elRef.nativeElement.contains(event.target)) {
      this.isOpen.set(false);
    }
  }

  toggle() {
    this.isOpen.update(v => !v);
  }

  select(sub: AvailableSubstituteDto) {
    this.substituteSelected.emit(sub.teacherId);
    this.isOpen.set(false);
    
    const teacher = this.substitutes().find(t => t.teacherId === sub.teacherId);
    console.log(`Selected substitute: ${teacher?.name}, Free Periods Today: ${teacher?.freePeriodsToday}`);
    if (teacher && teacher.freePeriodsToday === 1) {
      this.showWarning();
    }
  }

  private showWarning(): void {
    this.warningVisible.set(true);
  }

  getLoadClass(effectiveLoad: number): string {
    if (effectiveLoad <= 4) return 'load-low';
    if (effectiveLoad <= 8) return 'load-medium';
    return 'load-high';
  }
}
