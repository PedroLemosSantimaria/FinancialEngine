import { Routes } from '@angular/router';

export const routes: Routes = [
  { path: '', redirectTo: 'accounts', pathMatch: 'full' },
  {
    path: 'accounts',
    loadComponent: () =>
      import('./features/accounts/accounts-list/accounts-list.component').then(m => m.AccountsListComponent)
  },
  {
    path: 'accounts/:id/statement',
    loadComponent: () =>
      import('./features/statement/statement.component').then(m => m.StatementComponent)
  },
  {
  path: 'accounts/:id/new-transaction',
  loadComponent: () =>
    import('./features/transaction-form/transaction-form.component').then(m => m.TransactionFormComponent)
},
  { path: '**', redirectTo: 'accounts' }
];