import { HttpClient } from "@angular/common/http";
import { inject, Injectable } from "@angular/core";
import { CreateTimetableEntryDto, TimetableEntryDto, UpdateTimetableEntryDto } from "../model/time-table.model";
import { Observable } from "rxjs";
import { TodaySubstitutionDto } from "../model/absence.model";

@Injectable({ providedIn: 'root' })
export class TimetableService {
  private http = inject(HttpClient);
  private baseUrl = 'http://localhost:5179';

  getTeacherTimetable(teacherId: number): Observable<TimetableEntryDto[]> {
    return this.http.get<TimetableEntryDto[]>(`${this.baseUrl}/api/teachers/${teacherId}/timetable`);
  }

  createEntry(payload: CreateTimetableEntryDto): Observable<TimetableEntryDto> {
    return this.http.post<TimetableEntryDto>(`${this.baseUrl}/api/timetable`, payload);
  }

  updateEntry(id: number, payload: UpdateTimetableEntryDto): Observable<TimetableEntryDto> {
    return this.http.put<TimetableEntryDto>(`${this.baseUrl}/api/timetable/${id}`, payload);
  }

  deleteEntry(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/api/timetable/${id}`);
  }

  todaySubstitutions(teacherId: number): Observable<TodaySubstitutionDto[]> {
    return this.http.get<TodaySubstitutionDto[]>(`${this.baseUrl}/api/substitutes/today/${teacherId}`);
  }
}