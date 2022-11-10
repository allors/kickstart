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
  FilterDefinition,
  FilterField,
  FilterService,
  MediaService,
  RefreshService,
  SearchFactory,
  Table,
  TableRow,
} from '@allors/base/workspace/angular/foundation';
import {
  SerialisedItem,
  WorkEffort,
  WorkEffortState,
} from '@allors/default/workspace/domain';
import { M } from '@allors/default/workspace/meta';
import {
  DeleteActionService,
  OverviewActionService,
  SorterService,
} from '@allors/base/workspace/angular-material/application';
import { NavigationService } from '@allors/base/workspace/angular/application';
import { And, Equals } from '@allors/system/workspace/domain';
import { formatDistance } from 'date-fns';
import { CustomerOrganisationId } from '../../../services/state/customer-organisation-id';

interface Row extends TableRow {
  object: WorkEffort;
  number: string;
  name: string;
  state: string;
  equipment: string;
  executedBy: string;
  lastModifiedDate: string;
}

@Component({
  templateUrl: './workeffort-list-page.component.html',
  providers: [ContextService],
})
export class WorkEffortListPageComponent implements OnInit, OnDestroy {
  public title = 'Work Orders';

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
    public customerOrganisationId: CustomerOrganisationId,
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
        { name: 'name', sort: true },
        { name: 'state' },
        { name: 'executedBy' },
        { name: 'equipment' },
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

    const workEffortStateSearch = new SearchFactory({
      objectType: m.WorkEffortState,
      roleTypes: [m.WorkEffortState.Name],
    });

    const serialisedItemSearch = new SearchFactory({
      objectType: m.SerialisedItem,
      predicates: [
        {
          kind: 'And',
          operands: [
            {
              kind: 'Or',
              operands: [
                {
                  kind: 'Equals',
                  propertyType: m.SerialisedItem.OwnedBy,
                  value: this.customerOrganisationId.value,
                },
                {
                  kind: 'Equals',
                  propertyType: m.SerialisedItem.RentedBy,
                  value: this.customerOrganisationId.value,
                },
              ],
            },
          ],
        },
      ],
      roleTypes: [m.FixedAsset.SearchString],
    });

    const filterdefinition = new FilterDefinition(
      {
        kind: 'And',
        operands: [
          {
            kind: 'Equals',
            propertyType: m.WorkEffort.WorkEffortState,
            parameter: 'state',
          },
          {
            kind: 'Like',
            roleType: m.WorkEffort.WorkEffortNumber,
            parameter: 'Number',
          },
          { kind: 'Like', roleType: m.WorkEffort.Name, parameter: 'Name' },
          {
            kind: 'Like',
            roleType: m.WorkEffort.Description,
            parameter: 'Description',
          },
          {
            kind: 'ContainedIn',
            propertyType:
              m.WorkEffort.WorkEffortFixedAssetAssignmentsWhereAssignment,
            extent: {
              kind: 'Filter',
              objectType: m.WorkEffortFixedAssetAssignment,
              predicate: {
                kind: 'Equals',
                propertyType: m.WorkEffortFixedAssetAssignment.FixedAsset,
                parameter: 'equipment',
              },
            },
          },
        ],
      },
      {
        state: {
          search: () => workEffortStateSearch,
          display: (v: WorkEffortState) => v && v.Name,
        },
        equipment: {
          search: () => serialisedItemSearch,
          display: (v: SerialisedItem) => v && v.DisplayName,
        },
      }
    );

    this.filter = new Filter(filterdefinition);

    const customerOrganisationPredicate: Equals = {
      kind: 'Equals',
      propertyType: m.WorkEffort.Customer,
    };
    const predicate: And = {
      kind: 'And',
      operands: [
        customerOrganisationPredicate,
        this.filter.definition.predicate,
      ],
    };

    this.subscription = combineLatest([
      this.refreshService.refresh$,
      this.filter.fields$,
      this.table.sort$,
      this.table.pager$,
      this.customerOrganisationId.observable$,
    ])
      .pipe(
        scan(
          (
            [previousRefresh, previousFilterFields],
            [refresh, filterFields, sort, pageEvent, customerOrganisationId]
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
              customerOrganisationId,
            ];
          }
        ),
        switchMap(
          ([, filterFields, sort, pageEvent, customerOrganisationId]: [
            Date,
            FilterField[],
            Sort,
            PageEvent,
            number
          ]) => {
            customerOrganisationPredicate.value = customerOrganisationId;

            const pulls = [
              pull.WorkEffort({
                predicate,
                sorting: sort
                  ? this.sorterService.sorter(m.WorkEffort)?.create(sort)
                  : null,
                include: {
                  Customer: x,
                  ExecutedBy: x,
                  PrintDocument: {
                    Media: x,
                  },
                  WorkEffortState: x,
                  WorkEffortPurposes: x,
                  WorkEffortFixedAssetAssignmentsWhereAssignment: {
                    FixedAsset: x,
                  },
                  WorkEffortPartyAssignmentsWhereAssignment: {
                    Party: x,
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
        const workEfforts = loaded.collection<WorkEffort>(m.WorkEffort);
        this.table.data = workEfforts
          ?.filter((v) => v.canReadWorkEffortNumber)
          ?.map((v) => {
            return {
              object: v,
              number: v.WorkEffortNumber,
              name: v.Name,
              state: v.WorkEffortState ? v.WorkEffortState.Name : '',
              executedBy: v.ExecutedBy ? v.ExecutedBy.DisplayName : '',
              equipment: v.WorkEffortFixedAssetAssignmentsWhereAssignment
                ? v.WorkEffortFixedAssetAssignmentsWhereAssignment?.map(
                    (w) => w.FixedAsset.DisplayName
                  ).join(', ')
                : '',
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
