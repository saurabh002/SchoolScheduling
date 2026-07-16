export interface SubstituteWorkload {
  teacherId: number;
  name: string;
  regularLoad: number;
  subsTaken: number;
  missedClasses: number;
  effectiveLoad: number;
}

export interface AbsenceTrendPoint {
  date: string;
  count: number;
}

export interface ReportSummary {
  totalTeachers: number;
  absencesToday: number;
  absencesThisWeek: number;
  periodsPendingToday: number;
  periodsAssignedToday: number;
  periodLabel: string;
  topSubstitutes: SubstituteWorkload[];
  absenceTrend: AbsenceTrendPoint[];
}

export type PeriodType = 'week' | 'month';