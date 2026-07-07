import { Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatSelectChange, MatSelectModule } from '@angular/material/select';
import { TeacherService } from '../services/teacher.service';
import { CreateTeacherDto } from '../model/teacherModel';
import { Router } from '@angular/router';

@Component({
  selector: 'app-teacher',
  imports: [ReactiveFormsModule, MatFormFieldModule,MatInputModule,
    MatSelectModule,MatIconModule,MatButtonModule],
  templateUrl: './teacher.html',
  styleUrl: './teacher.css',
})
export class Teacher {
  fb = inject(FormBuilder);
  teacherService = inject(TeacherService);
  router = inject(Router);

  teacherForm= this.fb.group({
    name: ['', Validators.required],
    phone: [''],
    email: [''],
    department: ['']
  });

  onSubmit(){
    if (this.teacherForm.invalid) return;

    const payload: CreateTeacherDto = {
      name: this.teacherForm.value.name!,
      department: this.teacherForm.value.department,
      email: this.teacherForm.value.email,
      phone: this.teacherForm.value.phone
    };
    
    this.teacherService.createTeacher(payload).subscribe({
      next: (createdTeacher) => {
        console.log('Teacher created:', createdTeacher);
        this.teacherForm.reset();
        this.router.navigate(['/timetable/'+createdTeacher.id]); // Navigate to the teachers list page after successful creation
      },
      error: (error) => {
        console.error('Error creating teacher:', error);
      }
    });
  }
}