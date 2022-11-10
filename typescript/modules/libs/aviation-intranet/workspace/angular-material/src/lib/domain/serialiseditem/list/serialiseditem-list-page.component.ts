import { Component, OnDestroy, OnInit, Self } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { Subscription, combineLatest } from 'rxjs';
import { switchMap, scan } from 'rxjs/operators';

import { Sort } from '@angular/material/sort';
import { PageEvent } from '@angular/material/paginator';
import {
  ContextService,
  Filter,
  FilterField,
  FilterService,
  MediaService,
  RefreshService,
  Table,
  TableRow,
} from '@allors/base/workspace/angular/foundation';
import { SerialisedItem } from '@allors/default/workspace/domain';
import { M } from '@allors/default/workspace/meta';
import {
  OverviewActionService,
  SorterService,
} from '@allors/base/workspace/angular-material/application';
import { NavigationService } from '@allors/base/workspace/angular/application';

interface Row extends TableRow {
  object: SerialisedItem;
  id: string;
  name: string;
  categories: string;
  availability: string;
  ownership: string;
  SN: string;
  ownedBy: string;
  rentedBy: string;
  facility: string;
  YOM: string;
  iataCode: string;
}

@Component({
  templateUrl: './serialiseditem-list-page.component.html',
  providers: [ContextService],
})
export class SerialisedItemListPageComponent implements OnInit, OnDestroy {
  public title = 'Serialised Assets';

  table: Table<Row>;

  private subscription: Subscription;
  filter: Filter;
  m: M;

  constructor(
    @Self() public allors: ContextService,
    public refreshService: RefreshService,
    public overviewService: OverviewActionService,
    public navigation: NavigationService,
    public mediaService: MediaService,
    public filterService: FilterService,
    public sorterService: SorterService,
    titleService: Title
  ) {
    this.allors.context.name = this.constructor.name;
    titleService.setTitle(this.title);

    this.m = this.allors.context.configuration.metaPopulation as M;

    this.table = new Table({
      selection: false,
      columns: [
        { name: 'id', sort: true },
        { name: 'name', sort: true },
        { name: 'categories', sort: true },
        { name: 'availability', sort: true },
        { name: 'ownership', sort: true },
        { name: 'SN' },
        { name: 'ownedBy', sort: true },
        { name: 'rentedBy', sort: true },
        { name: 'YOM', sort: true },
        { name: 'facility', sort: true },
        { name: 'iataCode', sort: true },
      ],
      actions: [overviewService.overview()],
      defaultAction: overviewService.overview(),
      pageSize: 50,
      initialSort: 'id',
    });
  }

  public ngOnInit(): void {
    const m = this.m;
    const { pullBuilder: pull } = m;
    const x = {};

    this.filter = this.filterService.filter(m.SerialisedItem);

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
              pull.SerialisedItem({
                predicate: this.filter.definition.predicate,
                sorting: sort
                  ? this.sorterService.sorter(m.SerialisedItem)?.create(sort)
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

        const objects = loaded.collection<SerialisedItem>(m.SerialisedItem);

        this.table.data = objects?.map((v) => {
          return {
            object: v,
            id: v.ItemNumber,
            name: v.PartName,
            categories: v.ProductCategoriesDisplayName,
            availability: v.SerialisedItemAvailabilityName,
            ownership: v.OwnershipName,
            SN: v.SerialNumber,
            ownedBy: v.OwnedByPartyName,
            rentedBy: v.RentedByPartyName,
            YOM: v.ManufacturingYear?.toString(),
            facility: v.Location,
            iataCode: v.IataCode,
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
