import { HttpClient } from "@angular/common/http";
import { inject, Injectable } from "@angular/core";
import { Observable } from "rxjs";
import {
  AssignedSubstitutionDto,
  CreateAbsenceDto,
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

  assignSubstitute(absencePeriodId: number, substituteTeacherId: number): Observable<{ id: number; teacherId: number; substituteTeacherId: number }> {
    return this.http.post<{ id: number; teacherId: number; substituteTeacherId: number }>(
      `${this.baseUrl}/api/substitutes/assign`,
      { absencePeriodId, substituteTeacherId }
    );
  }

  getExistingAbsence(teacherId: number, date: string) {
    return this.http.get<ExistingAbsenceDto>(`/api/absences?teacherId=${teacherId}&date=${date}`);
  }

  deleteAbsencePeriod(absencePeriodId: number) {
    return this.http.delete(`/api/absences/periods/${absencePeriodId}`);
  }

  deleteAbsence(absenceId: number) {
    return this.http.delete(`/api/absences/${absenceId}`);
  }
}