import { HttpClient } from "@angular/common/http";
import { inject, Injectable } from "@angular/core";
import { Observable } from "rxjs";
import {
  AssignedSubstitutionDto,
  CreateAbsenceDto,
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
}