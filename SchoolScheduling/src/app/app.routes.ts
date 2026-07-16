import { Routes } from '@angular/router';
import { App } from './app';
import { Teacher } from './teacher/teacher';

export const routes: Routes = [
    {path:'teacher', component: Teacher},
    {path:'teacher/:id', component: Teacher},
    {path:'timetable', loadComponent: () => import('./time-table/time-table').then(m => m.TimeTable)},
    {path:'timetable/:teacherId', loadComponent: () => import('./time-table/time-table').then(m => m.TimeTable)},
    {
        path: 'absence',
        loadComponent: () => import('./absence/absence')
        .then(m => m.Absence)
    },
    {
        path: 'substitutions',
        loadComponent: () => import('./substitutions/substitutions')
        .then(m => m.Substitutions)
    },
    {
        path: 'report',
        loadComponent: () => import('./report/report')
        .then(m => m.Report)
    }
];
