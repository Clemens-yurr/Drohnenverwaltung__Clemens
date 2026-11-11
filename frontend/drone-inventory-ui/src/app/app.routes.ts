import { Routes } from '@angular/router';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'drones' },
  {
    path: 'drones',
    title: 'Drones',
    loadComponent: () => import('./features/drones/drone-page').then((module) => module.DronePage),
  },
  {
    path: 'student-groups',
    title: 'Student groups',
    loadComponent: () =>
      import('./features/student-groups/student-group-page').then(
        (module) => module.StudentGroupPage,
      ),
  },
  {
    path: 'loans',
    title: 'Active loans',
    loadComponent: () =>
      import('./features/loans/loan-list-page').then((module) => module.LoanListPage),
  },
  {
    path: 'loans/new',
    title: 'New loan',
    loadComponent: () =>
      import('./features/loans/loan-form-page').then((module) => module.LoanFormPage),
  },
  {
    path: 'loans/history',
    title: 'Loan history',
    loadComponent: () =>
      import('./features/loans/loan-history-page').then((module) => module.LoanHistoryPage),
  },
  { path: '**', redirectTo: 'drones' },
];
