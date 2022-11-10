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
import {
  InternalOrganisation,
  Singleton,
} from '@allors/default/workspace/domain';
import {
  FetcherService,
  InternalOrganisationId,
} from '@allors/apps-intranet/workspace/angular-material';

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
export class DashboardComponent implements OnInit, OnDestroy {
  public m: M;
  singleton: Singleton;

  monthlyInvoiceResult: string;
  purchaseInvoiceResult: string;
  salesInvoiceResult: string;

  private subscription: Subscription;
  internalOrganisation: InternalOrganisation;

  constructor(
    @Self() private allors: ContextService,
    public refreshService: RefreshService,
    private singletonId: SingletonId,
    private errorService: ErrorService,
    private fetcher: FetcherService,
    private internalOrganisationId: InternalOrganisationId,
    private titleService: Title
  ) {
    this.titleService.setTitle('Dashboard');
    this.m = this.allors.context.configuration.metaPopulation as M;
  }

  // TODO: move to login.component
  ngOnInit(): void {
    const m = this.m;
    const { pullBuilder: pull } = m;
    const x = {};

    this.subscription = combineLatest([
      this.refreshService.refresh$,
      this.internalOrganisationId.observable$,
    ])
      .pipe(
        switchMap(() => {
          const pulls = [
            this.fetcher.internalOrganisation,
            pull.Singleton({
              objectId: this.singletonId.value,
            }),
          ];

          return this.allors.context.pull(pulls);
        })
      )
      .subscribe((loaded) => {
        this.allors.context.reset();

        this.singleton = loaded.object<Singleton>(m.Singleton);
        this.internalOrganisation =
          this.fetcher.getInternalOrganisation(loaded);
      });
  }

  ngOnDestroy() {
    if (this.subscription) {
      this.subscription.unsubscribe();
    }
  }

  monthlyInvoice(): void {
    this.monthlyInvoiceResult = `Starting for ${this.internalOrganisation.DisplayName}`;
    this.allors.context
      .invoke(this.internalOrganisation.CreateWorkEffortInvoice)
      .subscribe(() => {
        this.monthlyInvoiceResult += '\nFinished';
      }, this.errorService.errorHandler);
  }

  salesInvoice() {
    this.salesInvoiceResult = 'Starting';
    this.allors.context
      .invoke(this.singleton.RepeatingSalesInvoicing)
      .subscribe(() => {
        this.salesInvoiceResult += '\nFinished';
      }, this.errorService.errorHandler);
  }

  purchaseInvoice() {
    this.purchaseInvoiceResult = 'Starting';
    this.allors.context
      .invoke(this.singleton.RepeatingPurchaseInvoicing)
      .subscribe(() => {
        this.purchaseInvoiceResult += '\nFinished';
      }, this.errorService.errorHandler);
  }
}
