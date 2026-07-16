import { Component, inject, input } from '@angular/core';
import { TodaySubstitutionDto } from '../../model/absence.model';
import { rxResource } from '@angular/core/rxjs-interop';
import { HttpClient } from '@angular/common/http';
import { TimetableService } from '../../services/time-table.service';

@Component({
  selector: 'app-today-substitutions',
  imports: [],
  templateUrl: './today.substitutions.html',
  styleUrl: './today.substitutions.css',
})
export class TodaySubstitutions {
  teacherId = input.required<number>();
  teacherService = inject(TimetableService);
  private readonly http = inject(HttpClient);
 
  substitutionsResource = rxResource({
    params: () => ({ id: this.teacherId() }),
    stream: ({ params }) =>
      this.teacherService.todaySubstitutions(params.id)
  });
}
