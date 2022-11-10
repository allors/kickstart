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
import { Vehicle } from '@allors/default/workspace/domain';
import { M } from '@allors/default/workspace/meta';
import {
  DeleteActionService,
  EditActionService,
  OverviewActionService,
  SorterService,
} from '@allors/base/workspace/angular-material/application';
import { NavigationService } from '@allors/base/workspace/angular/application';

interface Row extends TableRow {
  object: Vehicle;
  licensePlateNumber: string;
  make: string;
  model: string;
  ownedBy: string;
}

@Component({
  templateUrl: './vehicle-list-page.component.html',
  providers: [ContextService],
})
export class VehicleListPageComponent implements OnInit, OnDestroy {
  public title = 'Vehicles';

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
        { name: 'licensePlateNumber', sort: true },
        { name: 'make', sort: true },
        { name: 'model', sort: true },
        { name: 'ownedBy', sort: true },
      ],
      actions: [overviewService.overview(), this.delete],
      defaultAction: overviewService.overview(),
      pageSize: 50,
      initialSort: 'licensePlateNumber',
    });
  }

  public ngOnInit(): void {
    const m = this.m;
    const { pullBuilder: pull } = m;

    this.filter = this.filterService.filter(m.Vehicle);

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
              pull.Vehicle({
                predicate: this.filter.definition.predicate,
                sorting: sort
                  ? this.sorterService.sorter(m.Vehicle)?.create(sort)
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

        const objects = loaded.collection<Vehicle>(m.Vehicle);

        this.table.data = objects?.map((v) => {
          return {
            object: v,
            licensePlateNumber: v.LicensePlateNumber,
            make: v.Make,
            model: v.Model,
            ownedBy: v.OwnedByPartyName,
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
