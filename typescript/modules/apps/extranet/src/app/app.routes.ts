import { Routes } from '@angular/router';
import { AuthorizationService } from './auth/authorization.service';

import { LoginComponent } from './auth/login.component';
import { ErrorComponent } from './error/error.component';
import { MainComponent } from './main/main.component';
import { DashboardComponent } from './dashboard/dashboard.component';
import {} from '@allors/apps-extranet/workspace/angular-material';

import {
  RedirectComponent,
  SerialisedItemListPageComponent,
  SerialisedItemOverviewPageComponent,
  WorkEffortListPageComponent,
  WorkRequirementListPageComponent,
} from '@allors/aviation-extranet/workspace/angular-material';

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
            path: 'products',
            children: [
              {
                path: 'serialiseditems',
                component: SerialisedItemListPageComponent,
              },
              {
                path: 'serialisedItem/:id',
                component: SerialisedItemOverviewPageComponent,
              },
            ],
          },
          {
            path: 'workefforts',
            children: [
              {
                path: 'workrequirements',
                component: WorkRequirementListPageComponent,
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
  SerialisedItemListPageComponent,
  SerialisedItemOverviewPageComponent,
  WorkEffortListPageComponent,
  WorkRequirementListPageComponent,
];
