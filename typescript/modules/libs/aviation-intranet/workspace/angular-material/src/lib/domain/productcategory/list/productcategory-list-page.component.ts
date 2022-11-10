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
import { ProductCategory } from '@allors/default/workspace/domain';
import { M } from '@allors/default/workspace/meta';
import {
  DeleteActionService,
  EditActionService,
  OverviewActionService,
  SorterService,
} from '@allors/base/workspace/angular-material/application';
import { NavigationService } from '@allors/base/workspace/angular/application';
import { InternalOrganisationId } from '@allors/apps-intranet/workspace/angular-material';

interface Row extends TableRow {
  object: ProductCategory;
  name: string;
  family: string;
  group: string;
  iataCode: string;
  scope: string;
}

@Component({
  templateUrl: './productcategory-list-page.component.html',
  providers: [ContextService],
})
export class ProductCategoryListPageComponent implements OnInit, OnDestroy {
  public title = 'Categories';

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
    public editRoleService: EditActionService,
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

    this.edit = editRoleService.edit();
    this.edit.result.subscribe(() => {
      this.table.selection.clear();
    });

    this.delete = deleteService.delete();
    this.delete.result.subscribe(() => {
      this.table.selection.clear();
    });

    this.table = new Table({
      selection: true,
      columns: [
        { name: 'name', sort: true },
        { name: 'family', sort: true },
        { name: 'group', sort: true },
        { name: 'iataCode', sort: true },
        { name: 'scope', sort: true },
      ],
      actions: [this.edit, this.delete],
      defaultAction: this.edit,
      pageSize: 50,
    });
  }

  ngOnInit(): void {
    const m = this.m;
    const { pullBuilder: pull } = m;
    const x = {};

    this.filter = this.filterService.filter(m.ProductCategory);

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
          ([, filterFields, sort, pageEvent]: [
            Date,
            FilterField[],
            Sort,
            PageEvent
          ]) => {
            const pulls = [
              pull.ProductCategory({
                predicate: this.filter.definition.predicate,
                sorting: sort
                  ? this.sorterService.sorter(m.ProductCategory)?.create(sort)
                  : null,
                include: {
                  CategoryImage: x,
                  LocalisedNames: x,
                  LocalisedDescriptions: x,
                  CatScope: x,
                  IataGseCode: x,
                  PrimaryParent: {
                    PrimaryParent: x,
                  },
                },
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

        const objects = loaded.collection<ProductCategory>(m.ProductCategory);
        this.table.data = objects?.map((v) => {
          return {
            object: v,
            name: v.Name,
            family:
              (v.IsGroup && v.PrimaryParent.DisplayName) ||
              (v.IsSubGroup && v.PrimaryParent.PrimaryParent.DisplayName) ||
              '',
            group: (v.IsSubGroup && v.PrimaryParent.DisplayName) || '',
            iataCode: v.IataGseCode && v.IataGseCode.Code,
            scope: v.CatScope?.Name,
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
