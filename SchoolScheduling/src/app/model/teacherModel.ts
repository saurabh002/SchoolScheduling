export interface TeacherModel {
  id: number;
  name: string;
  department?: string;
  email?: string;
  phone?: string;
  isActive: boolean;
}

export interface CreateTeacherDto {
  id?: number;
  name: string;
  department?: string | null;
  email?: string | null;
  phone?: string | null;
}