// models/timetable-entry.model.ts
export interface TimetableEntryDto {
  id: number;
  teacherId: number;
  classSectionId: number;
  classSectionName: string;
  periodId: number;
  periodNumber: number;
  startTime: string;
  endTime: string;
  dayOfWeek: number;
  subject: string | null;
}

export interface CreateTimetableEntryDto {
  teacherId: number;
  classSectionId: number;
  periodId: number;
  dayOfWeek: number;
  subject: string | null;
}

export interface UpdateTimetableEntryDto {
  teacherId: number;
  classSectionId: number;
  periodId: number;
  dayOfWeek: number;
  subject: string | null;
}