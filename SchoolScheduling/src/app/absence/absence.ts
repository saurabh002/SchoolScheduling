import { Component, computed, inject, signal } from '@angular/core';
import { TeacherService } from '../services/teacher.service';
import { rxResource } from '@angular/core/rxjs-interop';
import { TeacherModel } from '../model/teacherModel';
import { TimetableService } from '../services/time-table.service';
import { map } from 'rxjs';
import { TimetableEntryDto } from '../model/time-table.model';
import { AbsenceService } from '../services/absence.service';
import { CreateAbsenceDto, ExistingAbsenceDto } from '../model/absence.model';
import { Router } from '@angular/router';
import { DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { firstValueFrom } from 'rxjs';

interface PeriodRow {
  timetableEntryId: number;
  periodId: number;
  periodNumber: number;
  startTime: string;
  endTime: string;
  classSectionName: string;
  subject: string | null;
  checked: boolean;
}

@Component({
  selector: 'app-absence',
  imports: [DatePipe, FormsModule],
  templateUrl: './absence.html',
  styleUrl: './absence.css',
})
export class Absence {
  private teacherService = inject(TeacherService);
  private timetableService = inject(TimetableService);
  private absenceService = inject(AbsenceService);
  router = inject(Router);

  teacherSearch = signal<string>('');
  selectedTeacher = signal<TeacherModel | null>(null);
  selectedDate = signal<string | null>(null);
  scheduleLoaded = signal<boolean>(false);
  scheduleLoading = signal<boolean>(false);
  periodRows = signal<PeriodRow[]>([]);
  errorMessage = signal<string | null>(null);
  existingAbsence = signal<ExistingAbsenceDto | null>(null);

  teachersResource = rxResource<TeacherModel[], void>({
    stream: () => this.teacherService.getTeachers()
  });

  filteredTeachers = computed(() => {
    const search = this.teacherSearch().toLowerCase().trim();
    if (!search || this.selectedTeacher()) return [];
    return (this.teachersResource.value() ?? [])
      .filter(t => t.name.toLowerCase().includes(search));
  });

  checkedPeriods = computed(() =>
    this.periodRows().filter(p => p.checked)
  );

  // timetableEntryId -> AbsencePeriodDto for quick lookup
  existingPeriodMap = computed(() => {
    const absence = this.existingAbsence();
    if (!absence) return new Map<number, { id: number; hasSubstitute: boolean }>();
    return new Map(absence.periods.map(p => [p.timetableEntryId, { id: p.id, hasSubstitute: p.hasSubstitute }]));
  });

  // true if any period has a substitute assigned
  anySubAssigned = computed(() =>
    this.existingAbsence()?.periods.some(p => p.hasSubstitute) ?? false
  );

  // all periods on this day are already marked absent
  allPeriodsAbsent = computed(() => {
    const rows = this.periodRows();
    const map = this.existingPeriodMap();
    return rows.length > 0 && rows.every(r => map.has(r.timetableEntryId));
  });

  // absence exists at all
  hasExistingAbsence = computed(() => this.existingAbsence() !== null);

  togglePeriodSelection(period: PeriodRow) {
    this.periodRows.update(rows =>
      rows.map(row =>
        row.timetableEntryId === period.timetableEntryId
          ? { ...row, checked: !row.checked }
          : row
      )
    );
  }

  selectTeacher(teacher: TeacherModel) {
    this.selectedTeacher.set(teacher);
    this.teacherSearch.set(teacher.name);
    this.scheduleLoaded.set(false);
    this.periodRows.set([]);
    this.existingAbsence.set(null);
    this.errorMessage.set(null);
  }

  loadSchedules() {
    const teacher = this.selectedTeacher();
    const date = this.selectedDate();
    if (!teacher || !date) return;

    this.scheduleLoading.set(true);
    this.errorMessage.set(null);
    const dayNumber = new Date(date).getDay();

    this.timetableService.getTeacherTimetable(teacher.id).pipe(
      map((entries: TimetableEntryDto[]) =>
        entries
          .filter(e => e.dayOfWeek === dayNumber)
          .map((e): PeriodRow => ({
            timetableEntryId: e.id,
            periodId: e.periodId,
            periodNumber: e.periodNumber,
            startTime: e.startTime,
            endTime: e.endTime,
            classSectionName: e.classSectionName,
            subject: e.subject,
            checked: true
          }))
      )
    ).subscribe({
      next: (rows) => {
        this.scheduleLoading.set(false);
        if (rows.length === 0) {
          this.scheduleLoaded.set(false);
          this.errorMessage.set('No classes scheduled for this teacher on the selected date.');
          return;
        }
        this.periodRows.set(rows);
        this.scheduleLoaded.set(true);
        // check for existing absence after schedule loads
        this.checkExistingAbsence(teacher.id, date);
      },
      error: () => {
        this.scheduleLoading.set(false);
        this.errorMessage.set('Failed to load schedule. Please try again.');
      }
    });
  }

  private checkExistingAbsence(teacherId: number, date: string) {
    this.absenceService.getExistingAbsence(teacherId, date).subscribe({
      next: (absence) => this.existingAbsence.set(absence),
      error: (err) => {
        // 404 means no absence exists — that's fine
        if (err.status !== 404) {
          this.errorMessage.set('Failed to check existing absence.');
        }
      }
    });
  }

  saveAbsence() {
    const teacher = this.selectedTeacher();
    const date = this.selectedDate();
    const checked = this.checkedPeriods();
    const existing = this.existingAbsence();

    if (!teacher || !date) return;
    if (checked.length === 0) {
      this.errorMessage.set('Please select at least one period.');
      return;
    }

    this.errorMessage.set(null);

    const payload: CreateAbsenceDto = {
      teacherId: teacher.id,
      date,
      affectedPeriods: checked.map(p => ({
        timetableEntryId: p.timetableEntryId,
        periodId: p.periodId
      }))
    };

    if (existing) {
      this.absenceService.addPeriodsToAbsence(existing.id, payload.affectedPeriods).subscribe({
        next: () => this.checkExistingAbsence(teacher.id, date),
        error: (err) => {
          if (err.status === 409) {
            this.errorMessage.set(err.error?.message ?? 'Conflicting periods found.');
          } else {
            this.errorMessage.set('Failed to update absence. Please try again.');
          }
        }
      });
      return;
    }

    this.absenceService.createAbsence(payload).subscribe({
      next: () => {
        this.checkExistingAbsence(teacher.id, date);
      },
      error: (err) => {
        if (err.status === 409) {
          this.errorMessage.set(err.error?.message ?? 'Teacher already marked absent on this date.');
        } else {
          this.errorMessage.set('Failed to save absence. Please try again.');
        }
      }
    });
  }

  async removePeriodAbsence(timetableEntryId: number) {
    const periodInfo = this.existingPeriodMap().get(timetableEntryId);
    if (!periodInfo) return;

    try {
      await firstValueFrom(this.absenceService.deleteAbsencePeriod(periodInfo.id));
      // reload existing absence state
      const teacher = this.selectedTeacher();
      const date = this.selectedDate();
      if (teacher && date) this.checkExistingAbsence(teacher.id, date);
    } catch (err: any) {
      if (err.status === 409) {
        this.errorMessage.set(err.error?.message ?? 'Substitute already assigned. Please unassign first.');
      } else {
        this.errorMessage.set('Failed to remove period. Please try again.');
      }
    }
  }

  async cancelAllAbsence() {
    const absence = this.existingAbsence();
    if (!absence) return;

    try {
      await firstValueFrom(this.absenceService.deleteAbsence(absence.id));
      this.existingAbsence.set(null);
      this.errorMessage.set(null);
    } catch (err: any) {
      if (err.status === 409) {
        this.errorMessage.set(err.error?.message ?? 'Substitute already assigned. Please unassign first.');
      } else {
        this.errorMessage.set('Failed to cancel absence. Please try again.');
      }
    }
  }
}