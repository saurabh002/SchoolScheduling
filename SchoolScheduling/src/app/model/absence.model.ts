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
  id: number;
  name: string;
  totalClasses: number;
  substitutionCount: number;
  absenceCount: number;
  effectiveLoad: number;
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
