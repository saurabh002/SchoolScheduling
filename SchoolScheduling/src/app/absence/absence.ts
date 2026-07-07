import { Component, computed, DestroyRef, inject, signal } from '@angular/core';
import { TeacherService } from '../services/teacher.service';
import { rxResource, takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { TeacherModel } from '../model/teacherModel';
import { TimetableService } from '../services/time-table.service';
import { map } from 'rxjs';
import { TimetableEntryDto } from '../model/time-table.model';
import { AbsenceService } from '../services/absence.service';
import { CreateAbsenceDto } from '../model/absence.model';
import { Router } from '@angular/router';
import { DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';

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
 teacherService = inject(TeacherService);
 teacherSearch = signal<string>('');
 selectedTeacher = signal<TeacherModel | null>(null);
 selectedDate = signal<string | null>(null);
 timetableService = inject(TimetableService);
 private destroyRef = inject(DestroyRef);
 scheduleLoaded = signal<boolean>(false);
 periodRows = signal<PeriodRow[]>([]);
 absenceService = inject(AbsenceService);
 router = inject(Router);
 
 teachersResource = rxResource({
    stream: () => this.teacherService.getTeachers()
  });
  
 filteredTeachers = computed(() => {
    const search = this.teacherSearch().toLowerCase().trim();
    if (!search) return [];
    return (this.teachersResource.value() ?? [])
    .filter(teacher => teacher.name.toLowerCase().includes(search));
  })
 
  checkedPeriods = computed(() =>
    this.periodRows().filter(p => p.checked)
  );

  togglePeriodSelection(period: PeriodRow) {
    const updatedRows = this.periodRows().map(row => {
      if (row.timetableEntryId === period.timetableEntryId) {
        return { ...row, checked: !row.checked };
      }
      return row;
    });
    this.periodRows.set(updatedRows);
  }

  selectTeacher(teacher: TeacherModel) {
    this.selectedTeacher.set(teacher);
    this.teacherSearch.set(teacher.name);
    this.scheduleLoaded.set(false);
    this.periodRows.set([]);
  }

  loadSchedules() {
    const teacher = this.selectedTeacher();
    const date = this.selectedDate();
    if (teacher && date) {
      // Logic to load schedules for the selected teacher and date
      console.log(`Loading schedules for ${teacher.name} on ${date}`);
      const dayNumber = new Date(date).getDay();
      this.timetableService.getTeacherTimetable(teacher.id).pipe(
        map((entries: TimetableEntryDto[]): PeriodRow[] =>
          entries
            .filter((e: TimetableEntryDto) => e.dayOfWeek === dayNumber)
            .map((e: TimetableEntryDto): PeriodRow => ({
              timetableEntryId: e.id,
              periodId: e.periodId,
              periodNumber: e.periodNumber,
              startTime: e.startTime,
              endTime: e.endTime,
              classSectionName: e.classSectionName,
              subject: e.subject,
              checked: true
            }))
        ),
      takeUntilDestroyed(this.destroyRef)).subscribe({
        next: (rows: PeriodRow[]) => {
          if(rows.length === 0) {
            console.log('No timetable entries found for the selected date.');
            this.scheduleLoaded.set(false);
          } else {
            console.log('Timetable entries for the selected date:', rows);
            this.periodRows.set(rows);
            this.scheduleLoaded.set(true);
          }
        },
        error: () => {
          console.error('Error loading timetable entries:');
        }
    });
    } else {
      console.log('Please select both a teacher and a date.');
    }
  }

  saveAbsence() {
    const selectedTeacher = this.selectedTeacher();
    const selectedDate = this.selectedDate();
    const checkedPeriods = this.checkedPeriods();
    
    if (!selectedTeacher || !selectedDate) {
      console.log('Please select both a teacher and a date.');
      return;
    }

    if (checkedPeriods.length === 0) {
      console.log('Please select at least one period.');
      return;
    }

    console.log(`Saving absence for ${selectedTeacher.name} on ${selectedDate}`);
    console.log('Checked periods:', checkedPeriods);

    const payload: CreateAbsenceDto = {
      teacherId: selectedTeacher.id,
      date: selectedDate,
      affectedPeriods: checkedPeriods.map(p => ({
        timetableEntryId: p.timetableEntryId,
        periodId: p.periodId
      }))
    };

    this.absenceService.createAbsence(payload).subscribe({
      next: () => {
        this.router.navigate(['/substitutions']);
        console.log('Absence saved successfully.');
      },
      error: (err) => {
        console.error('Error saving absence.', err);
      }
    });
  }
}

