export interface CreateAbsencePeriodDto {
  timetableEntryId: number;
  periodId: number;
}

export interface CreateAbsenceDto {
  teacherId: number;
  date: string;
  affectedPeriods: CreateAbsencePeriodDto[];
}

export interface AvailableSubstituteDto {
  teacherId: number;
  name: string;
  regularLoad: number;
  subsTaken: number;
  missedClasses: number;
  effectiveLoad: number;
  freePeriodsToday: number;
}

export interface PendingSubstitutionDto {
  absencePeriodId: number;
  absentTeacherId: number;
  absentTeacherName: string;
  absentTeacherDepartment: string;
  periodId: number;
  periodNumber: number;
  startTime: string;
  endTime: string;
  classSectionName: string;
  subject: string;
  availableSubstitutes: AvailableSubstituteDto[];
}

export interface AssignedSubstitutionDto {
  absencePeriodId: number;
  absentTeacherId: number;
  absentTeacherName: string;
  substituteTeacherId: number;
  substituteTeacherName: string;
  periodNumber: number;
  startTime: string;
  endTime: string;
  classSectionName: string;
  subject: string;
}

export interface AbsencePeriodDto {
  id: number;
  timetableEntryId: number;
  hasSubstitute: boolean;
}

export interface ExistingAbsenceDto {
  id: number;
  periods: AbsencePeriodDto[];
}

export interface TodaySubstitutionDto {
  periodNumber: number;
  startTime: string;
  endTime: string;
  className: string;
  subject: string;
  absentTeacherName: string;
}