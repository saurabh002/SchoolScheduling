import { Component, effect, inject, input, numberAttribute, signal } from '@angular/core';
import { FormControl, FormsModule, ReactiveFormsModule } from '@angular/forms';
import { TeacherModel } from '../model/teacherModel';
import { TeacherService } from '../services/teacher.service';
import { TimetableService } from '../services/time-table.service';
import { Router } from '@angular/router';

interface Assignment {
  entryId: number;
  //gradeId: number;
  gradeName: string;
  subject: string | null;
}
@Component({
  selector: 'app-time-table',
  imports: [ReactiveFormsModule, FormsModule],
  templateUrl: './time-table.html',
  styleUrl: './time-table.css',
})
export class TimeTable {
  grades = ['1A', '1B', '2A', '2B', '3A', '3B', '4A', '4B', '5A', '5B'];
  subjects = ['Maths', 'English', 'EVS', 'ICT', 'Hindi', 'Science', 'Social', 'Art', 'PE'];
  periods = [1, 2, 3, 4, 5, 6, 7, 8];
  days = ['Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday'];
  private dayOfWeekMap: Record<string, number> = {
    Sunday: 0, Monday: 1, Tuesday: 2, Wednesday: 3,
    Thursday: 4, Friday: 5, Saturday: 6
  };
  private dayOfWeekReverseMap: Record<number, string> = {
    0: 'Sunday', 1: 'Monday', 2: 'Tuesday', 3: 'Wednesday',
    4: 'Thursday', 5: 'Friday', 6: 'Saturday'
  };
  assignments = signal<Map<string, Assignment>>(new Map());
  gradeControl = new FormControl('', { nonNullable: true });
  subjectControl = new FormControl('', { nonNullable: true });
  activeCell = signal<{ day: string, period: number } | null>(null); 
  teacherSearch = signal<string>('');
  teacherId = input<number | null, string | null>(null, { transform: numberAttribute });
  allTeachers = signal<TeacherModel[]>([]);
  teacherService = inject(TeacherService);
  timeTableService = inject(TimetableService);
  router = inject(Router);
  isSelected = signal<boolean>(false);

  constructor() {
    this.teacherService.getTeachers().subscribe(teachers => {
      this.allTeachers.set(teachers);
    });

    effect(() => {
      const id = this.teacherId();
      if (id !== null) {
        console.log('Loading timetable for teacher:', id);
        this.teacherService.getTeacher(id).subscribe(teacher => {
          this.teacherSearch.set(teacher.name);
          console.log('Teacher found:', teacher);
        });
      
        this.loadTeacherTimetable(id);
      }
      else {
        this.assignments.set(new Map());
      }
    });
  }

  filteredTeachers() {    
    const search = this.teacherSearch().toLowerCase();
    console.log('Searching for:', search);
    if (!search) return []; 
    return this.allTeachers().filter(teacher =>
      teacher.name.toLowerCase().includes(search)
    );
  } 

  onTeacherSearchChange() {
    this.isSelected.set(false);
  }

  selectTeacher(teacher: TeacherModel){
    this.router.navigate(['/timetable'], { queryParams: { teacherId: teacher.id } });  
    this.teacherSearch.set('');
    this.isSelected.set(true);
  }

  private loadTeacherTimetable(teacherId: number) {
    this.timeTableService.getTeacherTimetable(teacherId).subscribe(entries => {
      const map = new Map<string, Assignment>();
      entries.forEach(e => {
        const dayName = this.dayOfWeekReverseMap[e.dayOfWeek];
        const key = this.cellKey(dayName, e.periodNumber);
        map.set(key, {
          entryId: e.id,
          //gradeId: e.classSectionId,
          gradeName: e.classSectionName,
          subject: e.subject
        });
      });
      this.assignments.set(map);
    });
  }

  cellKey(day: string, period: number): string {
    return `${day}-${period}`;
  }

  openCell(day: string, period: number) {
    // const existing = this.assignments().get(this.cellKey(day, period));
    // this.gradeControl.setValue(existing?.gradeName ?? '');
    // this.subjectControl.setValue(existing?.subject ?? '');
    //this.cellError.set(null);
    this.activeCell.set({ day, period });
  }

  private gradeNameToId(gradeName: string): number {
    return this.grades.indexOf(gradeName) + 1;
  }

  confirmCell(): void {
    const cell = this.activeCell();
    const id = this.teacherId();
    if (id === null || !cell || !this.gradeControl.value || !this.subjectControl.value) {
      return; 
    }
    const payload = {
      teacherId: id,
      classSectionId: this.gradeNameToId(this.gradeControl.value),
      periodId: cell.period, // TEMP: assumes PeriodId === period number
      dayOfWeek: this.dayOfWeekMap[cell.day],
      subject: this.subjectControl.value
    };

    this.timeTableService.createEntry(payload).subscribe({
      next: (created) => {
        const map = new Map(this.assignments());
        map.set(this.cellKey(cell.day, cell.period), {
          entryId: created.id,
          //gradeId: payload.classSectionId,
          gradeName: this.gradeControl.value,
          subject: this.subjectControl.value
        });
        this.assignments.set(map);
        this.activeCell.set(null);
        //this.cellError.set(null);
      },
      error: (err) => {
        const msg = err?.error?.message ?? 'Could not save this slot.';
       // this.cellError.set(msg);
      }
    });
  }

  removeCell(day: string, period: number, event: Event): void {
    event.stopPropagation();
    const key = this.cellKey(day, period);
    const map = new Map(this.assignments());
    map.delete(key);
    this.assignments.set(map);
  }

  isActive(day: string, period: number) {
    const c = this.activeCell();
    console.log(c);
    return c?.day === day && c?.period === period;
  }
}