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
import { WorkRequirement } from '@allors/default/workspace/domain';
import { M } from '@allors/default/workspace/meta';
import {
  DeleteActionService,
  OverviewActionService,
  SorterService,
} from '@allors/base/workspace/angular-material/application';
import { NavigationService } from '@allors/base/workspace/angular/application';
import { InternalOrganisationId } from '@allors/apps-intranet/workspace/angular-material';
import { And, Equals } from '@allors/system/workspace/domain';
import { formatDistance } from 'date-fns';

interface Row extends TableRow {
  object: WorkRequirement;
  number: string;
  state: string;
  priority: string;
  originator: string;
  equipment: string;
  location: string;
  fleetCode: string;
  lastModifiedDate: string;
}

@Component({
  templateUrl: './workrequirement-list-page.component.html',
  providers: [ContextService],
})
export class WorkRequirementListPageComponent implements OnInit, OnDestroy {
  public title = 'Service Requests';

  table: Table<Row>;

  edit: Action;
  delete: Action;

  private subscription: Subscription;
  filter: Filter;
  m: M;

  constructor(
    @Self() public allors: ContextService,

    public refreshService: RefreshService,
    public overviewService: OverviewActionService,
    public deleteService: DeleteActionService,
    public navigation: NavigationService,
    private internalOrganisationId: InternalOrganisationId,
    public filterService: FilterService,
    public sorterService: SorterService,
    public mediaService: MediaService,
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
        { name: 'number', sort: true },
        { name: 'state', sort: true },
        { name: 'priority', sort: true },
        { name: 'originator', sort: true },
        { name: 'equipment', sort: true },
        { name: 'location', sort: true },
        { name: 'fleetCode', sort: true },
        { name: 'lastModifiedDate', sort: true },
      ],
      actions: [overviewService.overview(), this.delete],
      defaultAction: overviewService.overview(),
      pageSize: 50,
      initialSort: 'number',
      initialSortDirection: 'desc',
    });
  }

  public ngOnInit(): void {
    const m = this.m;
    const { pullBuilder: pull } = m;
    const x = {};

    this.filter = this.filterService.filter(m.WorkRequirement);

    const internalOrganisationPredicate: Equals = {
      kind: 'Equals',
      propertyType: m.WorkRequirement.ServicedBy,
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
              pull.WorkRequirement({
                predicate,
                sorting: sort
                  ? this.sorterService.sorter(m.WorkRequirement)?.create(sort)
                  : null,
                arguments: this.filter.parameters(filterFields),
                skip: pageEvent.pageIndex * pageEvent.pageSize,
                take: pageEvent.pageSize,
              }),
            ];

            return this.allors.context.pull(pulls);
          }
        )
      )
      .subscribe((loaded) => {
        this.allors.context.reset();
        const objects = loaded.collection<WorkRequirement>(m.WorkRequirement);
        this.table.data = objects?.map((v) => {
          return {
            object: v,
            number: v.RequirementNumber,
            state: v.RequirementStateName,
            priority: v.PriorityName,
            originator: v.OriginatorName,
            equipment: v.FixedAssetName,
            location: v.Location,
            fleetCode: v.FleetCode,
            lastModifiedDate: formatDistance(
              new Date(v.LastModifiedDate),
              new Date()
            ),
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
