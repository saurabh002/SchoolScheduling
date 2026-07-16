import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { PeriodType, ReportSummary } from '../model/report.model';

@Injectable({ providedIn: 'root' })
export class ReportService {
  private readonly http = inject(HttpClient);
  private baseUrl = 'http://localhost:5179';

  getSummary(periodType: PeriodType, offset: number): Observable<ReportSummary> {
    return this.http.get<ReportSummary>(`${this.baseUrl}/api/reports/summary`, {
      params: { periodType, offset }
    });
  }

  exportMonthlyReport(month: string): Observable<Blob> {
    return this.http.get(`${this.baseUrl}/api/reports/export`, {
      params: { month },
      responseType: 'blob',
    });
  }
}
 