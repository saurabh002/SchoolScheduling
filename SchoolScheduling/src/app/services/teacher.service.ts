import { HttpClient } from "@angular/common/http";
import { CreateTeacherDto, TeacherModel } from "../model/teacherModel";
import { inject, Injectable } from "@angular/core";
import { Observable } from "rxjs";

@Injectable({ providedIn: 'root' })

export class TeacherService {
    http = inject(HttpClient);
    private baseUrl = 'http://localhost:5179';

    getTeachers(): Observable<TeacherModel[]> {
        return this.http.get<TeacherModel[]>(`${this.baseUrl}/api/teachers`)
    }
    
    getTeacher(id: number | null): Observable<TeacherModel> {
        return this.http.get<TeacherModel>(`${this.baseUrl}/api/teachers/${id}`)
    }

    createTeacher(payload: CreateTeacherDto): Observable<TeacherModel> {
        return this.http.post<TeacherModel>(`${this.baseUrl}/api/teachers`, payload);
    }
}