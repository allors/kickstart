import { Component, OnDestroy, OnInit, Self } from '@angular/core';
import { Subscription, combineLatest } from 'rxjs';
import { switchMap } from 'rxjs/operators';

import { Title } from '@angular/platform-browser';
import {
  ContextService,
  ErrorService,
  RefreshService,
  SingletonId,
} from '@allors/base/workspace/angular/foundation';
import { M } from '@allors/default/workspace/meta';
import { Singleton } from '@allors/default/workspace/domain';

interface Item {
  title: string;
  subtitle: string;
  icon: string;
  routerLink: string[];
}

@Component({
  styleUrls: ['./dashboard.component.scss'],
  templateUrl: './dashboard.component.html',
  providers: [ContextService],
})
export class DashboardComponent {
  public m: M;

  private subscription: Subscription;

  constructor(
    @Self() private allors: ContextService,
    public refreshService: RefreshService,
    private errorService: ErrorService,
    private titleService: Title
  ) {
    this.titleService.setTitle('Dashboard');
    this.m = this.allors.context.configuration.metaPopulation as M;
  }
}
