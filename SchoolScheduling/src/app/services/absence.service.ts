import { HttpClient } from "@angular/common/http";
import { inject, Injectable } from "@angular/core";
import { Observable } from "rxjs";
import {
  AssignedSubstitutionDto,
  CreateAbsenceDto,
  CreateAbsencePeriodDto,
  ExistingAbsenceDto,
  PendingSubstitutionDto,
} from "../model/absence.model";

@Injectable({ providedIn: 'root' })
export class AbsenceService {
  private http = inject(HttpClient);
  private baseUrl = 'http://localhost:5179';

  createAbsence(payload: CreateAbsenceDto): Observable<{ id: number }> {
    return this.http.post<{ id: number }>(`${this.baseUrl}/api/absences`, payload);
  }

  addPeriodsToAbsence(absenceId: number, affectedPeriods: CreateAbsencePeriodDto[]): Observable<{ id: number; addedCount: number }> {
    return this.http.post<{ id: number; addedCount: number }>(
      `${this.baseUrl}/api/absences/absences/${absenceId}/periods`,
      { affectedPeriods }
    );
  }

  getPendingSubstitutions(date: string): Observable<PendingSubstitutionDto[]> {
    return this.http.get<PendingSubstitutionDto[]>(
      `${this.baseUrl}/api/substitutes/pending`,
      { params: { date } }
    );
  }

  getAssignedSubstitutions(date: string): Observable<AssignedSubstitutionDto[]> {
    return this.http.get<AssignedSubstitutionDto[]>(
      `${this.baseUrl}/api/substitutes/assigned`,
      { params: { date } }
    );
  }

  assignSubstitute(absencePeriodId: number, substituteTeacherId: number) {
    return this.http.put<{ id: number; substituteTeacherId: number }>(
      `${this.baseUrl}/api/absence-periods/${absencePeriodId}/assign`,
      { substituteTeacherId }
    );
  }

  getExistingAbsence(teacherId: number, date: string): Observable<ExistingAbsenceDto> {
    return this.http.get<ExistingAbsenceDto>(
      `${this.baseUrl}/api/absences/absences`,
      { params: { teacherId, date } }
    );
  }

  deleteAbsencePeriod(absencePeriodId: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/api/absences/absence-periods/${absencePeriodId}`);
  }

  deleteAbsence(absenceId: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/api/absences/absences/${absenceId}`);
  }

  unassign(absencePeriodId: number) {
    return this.http.delete(`${this.baseUrl}/api/substitutes/${absencePeriodId}/assign`);
  }
}