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
import { WorkEffortType } from '@allors/default/workspace/domain';
import { M } from '@allors/default/workspace/meta';
import {
  DeleteActionService,
  OverviewActionService,
  SorterService,
} from '@allors/base/workspace/angular-material/application';
import { NavigationService } from '@allors/base/workspace/angular/application';
import { format } from 'date-fns';

interface Row extends TableRow {
  object: WorkEffortType;
  from: string;
  through: string;
  name: string;
  product: string;
  category: string;
}

@Component({
  templateUrl: './workefforttype-list-page.component.html',
  providers: [ContextService],
})
export class WorkEffortTypeListPageComponent implements OnInit, OnDestroy {
  public title = 'Work Order Types';

  table: Table<Row>;

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
    public mediaService: MediaService,
    public filterService: FilterService,
    public sorterService: SorterService,
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
        { name: 'name', sort: true },
        { name: 'product', sort: true },
        { name: 'category', sort: true },
      ],
      actions: [overviewService.overview(), this.delete],
      defaultAction: overviewService.overview(),
      pageSize: 50,
      initialSort: 'name',
      initialSortDirection: 'asc',
    });
  }

  public ngOnInit(): void {
    const m = this.m;
    const { pullBuilder: pull } = m;
    const x = {};

    this.filter = this.filterService.filter(m.WorkEffortType);

    this.subscription = combineLatest([
      this.refreshService.refresh$,
      this.filter.fields$,
      this.table.sort$,
      this.table.pager$,
    ])
      .pipe(
        scan(
          (
            [previousRefresh, previousFilterFields],
            [refresh, filterFields, sort, pageEvent]
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

            return [refresh, filterFields, sort, pageEvent];
          }
        ),
        switchMap(
          ([, filterFields, sort, pageEvent]: [
            Date,
            FilterField[],
            Sort,
            PageEvent
          ]) => {
            const pulls = [
              pull.WorkEffortType({
                predicate: this.filter.definition.predicate,
                include: { UnifiedGood: x },
                sorting: sort
                  ? this.sorterService.sorter(m.WorkEffortType)?.create(sort)
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

        const workEffortTypes = loaded.collection<WorkEffortType>(
          m.WorkEffortType
        );
        this.table.data = workEffortTypes
          ?.filter((v) => v.canReadName)
          ?.map((v) => {
            return {
              object: v,
              from: format(new Date(v.FromDate), 'dd-MM-yyyy'),
              through:
                v.ThroughDate !== null
                  ? format(new Date(v.ThroughDate), 'dd-MM-yyyy')
                  : '',
              name: v.Name,
              product: v.UnifiedGoodDisplayName,
              category: v.ProductCategoryDisplayName,
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
