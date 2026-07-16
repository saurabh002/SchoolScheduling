import { Component, effect, inject, input, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
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
  id = input<number>();
  isUpdateMode = signal<boolean>(false);

  teacherForm= this.fb.group({
    firstName: ['', Validators.required],
    lastName: ['', Validators.required],
    phone: [''],
    email: [''],
    department: ['']
  });

  constructor() {
    effect(() => {
      const id = this.id();
      if (id) {
        this.isUpdateMode.set(true);
        this.teacherService.getTeacher(id).subscribe(teacher => {
          const [firstName, ...lastNameParts] = teacher.name.split(' ');
          const lastName = lastNameParts.join(' ');

          this.teacherForm.patchValue({
            firstName: firstName,
            lastName: lastName,
            phone: teacher.phone ?? '',
            email: teacher.email ?? '',
            department: teacher.department ?? ''
          });
        });
      }   
    });
  }

  onSubmit(){
    if (this.teacherForm.invalid) return;

    const firstName = this.teacherForm.value.firstName?.trim() ?? '';
    const lastName = this.teacherForm.value.lastName?.trim() ?? '';

    const payload: CreateTeacherDto = {
      id: this.id(),
      name: `${firstName} ${lastName}`.trim(),
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