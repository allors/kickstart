import { Routes } from '@angular/router';
import { AuthorizationService } from './auth/authorization.service';

import { LoginComponent } from './auth/login.component';
import { ErrorComponent } from './error/error.component';
import { MainComponent } from './main/main.component';
import { DashboardComponent } from './dashboard/dashboard.component';

import {
  EmailMessageListPageComponent,
  OrganisationListPageComponent,
  OrganisationOverviewPageComponent,
  PersonListPageComponent,
  PersonOverviewPageComponent,
  RedirectComponent,
} from '@allors/default/workspace/angular-material';

export const routes: Routes = [
  { path: 'login', component: LoginComponent },
  { path: 'error', component: ErrorComponent },
  {
    canActivate: [AuthorizationService],
    path: '',
    children: [
      {
        path: '',
        component: RedirectComponent,
        pathMatch: 'full',
      },
      {
        path: '',
        component: MainComponent,
        children: [
          {
            path: 'dashboard',
            component: DashboardComponent,
            pathMatch: 'full',
          },
          {
            path: 'contacts',
            children: [
              { path: 'people', component: PersonListPageComponent },
              { path: 'person/:id', component: PersonOverviewPageComponent },
              {
                path: 'organisations',
                component: OrganisationListPageComponent,
              },
              {
                path: 'organisation/:id',
                component: OrganisationOverviewPageComponent,
              },
            ],
          },
          {
            path: 'admin',
            children: [
              {
                path: 'emailmessages',
                component: EmailMessageListPageComponent,
              },
            ],
          },
        ],
      },
    ],
  },
];

export const components: any[] = [
  LoginComponent,
  ErrorComponent,
  MainComponent,
  DashboardComponent,
  PersonListPageComponent,
  PersonOverviewPageComponent,
  RedirectComponent,
];
