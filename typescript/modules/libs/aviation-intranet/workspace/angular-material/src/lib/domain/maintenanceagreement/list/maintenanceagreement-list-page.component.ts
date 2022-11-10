import { Component, OnDestroy, OnInit, Self } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { Subscription, combineLatest } from 'rxjs';
import { switchMap, scan } from 'rxjs/operators';

import { Sort } from '@angular/material/sort';
import { PageEvent } from '@angular/material/paginator';
import {
  Action,
  ContextService,
  Filter,
  FilterField,
  FilterService,
  MediaService,
  RefreshService,
  Table,
  TableRow,
} from '@allors/base/workspace/angular/foundation';
import { MaintenanceAgreement } from '@allors/default/workspace/domain';
import { M } from '@allors/default/workspace/meta';
import {
  DeleteActionService,
  OverviewActionService,
  SorterService,
} from '@allors/base/workspace/angular-material/application';
import { NavigationService } from '@allors/base/workspace/angular/application';
import { InternalOrganisationId } from '@allors/apps-intranet/workspace/angular-material';
import { And, Equals } from '@allors/system/workspace/domain';
import { format } from 'date-fns';

interface Row extends TableRow {
  object: MaintenanceAgreement;
  from: string;
  through: string;
  customer: string;

  type: string;
}

@Component({
  templateUrl: './maintenanceagreement-list-page.component.html',
  providers: [ContextService],
})
export class MaintenanceAgreementListPageComponent
  implements OnInit, OnDestroy
{
  public title = 'Service Agreements';
  m: M;

  table: Table<Row>;

  delete: Action;

  private subscription: Subscription;
  filter: Filter;
  constructor(
    @Self() public allors: ContextService,
    public refreshService: RefreshService,
    public overviewService: OverviewActionService,
    public deleteService: DeleteActionService,
    public navigation: NavigationService,
    public mediaService: MediaService,
    public filterService: FilterService,
    public sorterService: SorterService,
    private internalOrganisationId: InternalOrganisationId,

    titleService: Title
  ) {
    this.allors.context.name = this.constructor.name;
    titleService.setTitle(this.title);

    this.m = this.allors.context.configuration.metaPopulation as M;

    this.delete = deleteService.delete();
    this.delete.result.subscribe(() => {
      this.table.selection.clear();
    });

    this.table = new Table({
      selection: true,
      columns: [
        { name: 'from', sort: true },
        { name: 'through', sort: true },
        { name: 'customer', sort: true },
        { name: 'type' },
      ],
      actions: [overviewService.overview(), this.delete],
      defaultAction: overviewService.overview(),
      pageSize: 50,
      initialSort: 'from',
      initialSortDirection: 'desc',
    });
  }

  public ngOnInit(): void {
    const m = this.m;
    const { pullBuilder: pull } = m;
    const x = {};

    this.filter = this.filterService.filter(m.MaintenanceAgreement);

    const internalOrganisationPredicate: Equals = {
      kind: 'Equals',
      propertyType: m.MaintenanceAgreement.InternalOrganisation,
    };
    const predicate: And = {
      kind: 'And',
      operands: [
        internalOrganisationPredicate,
        this.filter.definition.predicate,
      ],
    };

    this.subscription = combineLatest([
      this.refreshService.refresh$,
      this.filter.fields$,
      this.table.sort$,
      this.table.pager$,
      this.internalOrganisationId.observable$,
    ])
      .pipe(
        scan(
          (
            [previousRefresh, previousFilterFields],
            [refresh, filterFields, sort, pageEvent, internalOrganisationId]
          ) => {
            pageEvent =
              previousRefresh !== refresh ||
              filterFields !== previousFilterFields
                ? {
                    ...pageEvent,
                    pageIndex: 0,
                  }
                : pageEvent;

            if (pageEvent.pageIndex === 0) {
              this.table.pageIndex = 0;
            }

            return [
              refresh,
              filterFields,
              sort,
              pageEvent,
              internalOrganisationId,
            ];
          }
        ),
        switchMap(
          ([, filterFields, sort, pageEvent, internalOrganisationId]: [
            Date,
            FilterField[],
            Sort,
            PageEvent,
            number
          ]) => {
            internalOrganisationPredicate.value = internalOrganisationId;

            const pulls = [
              pull.MaintenanceAgreement({
                predicate,
                include: {
                  WorkEffortType: x,
                },
                sorting: sort
                  ? this.sorterService
                      .sorter(m.MaintenanceAgreement)
                      ?.create(sort)
                  : null,
                arguments: this.filter.parameters(filterFields),
                skip: pageEvent.pageIndex * pageEvent.pageSize,
                take: pageEvent.pageSize,
              }),
              pull.WorkEffortType({}),
            ];

            return this.allors.context.pull(pulls);
          }
        )
      )
      .subscribe((loaded) => {
        this.allors.context.reset();
        const maintenanceAgreements = loaded.collection<MaintenanceAgreement>(
          m.MaintenanceAgreement
        );

        this.table.data = maintenanceAgreements
          ?.filter((v) => v.canReadFromDate)
          ?.map((v) => {
            return {
              object: v,
              from: format(new Date(v.FromDate), 'dd-MM-yyyy'),
              through:
                v.ThroughDate !== null
                  ? format(new Date(v.ThroughDate), 'dd-MM-yyyy')
                  : '',
              customer: v.CustomerName,
              type: v.WorkEffortType.Name,
            } as Row;
          });
      });
  }

  public ngOnDestroy(): void {
    if (this.subscription) {
      this.subscription.unsubscribe();
    }
  }
}
