import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    redirectTo: 'users/login',
    pathMatch: 'full',
  },
  {
    path: 'transactions',
    loadComponent: () =>
      import('./features/transactions/transactions.component').then(
        (m) => m.TransactionsComponent
      ),
  },
  {
    path: 'vaults',
    loadComponent: () =>
      import('./features/dashboard/vaults/vaults.component').then(
        (m) => m.VaultsComponent
      ),
  },
  {
    path: 'account-details',
    loadComponent: () =>
      import('./features/dashboard/account-details/account-details.component').then(
        (m) => m.AccountDetailsComponent
      ),
  },
  {
    path: 'users',
    loadChildren: () =>
      import('./features/users/users.routes').then((m) => m.USER_ROUTES),
  },
  {
    path: '**',
    redirectTo: 'users/login',
  },
];
