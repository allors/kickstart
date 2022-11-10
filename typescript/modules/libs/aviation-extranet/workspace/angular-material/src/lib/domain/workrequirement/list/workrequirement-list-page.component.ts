import { Subscription, combineLatest } from 'rxjs';
import { switchMap, scan } from 'rxjs/operators';
import { Component, OnDestroy, OnInit, Self } from '@angular/core';
import { PageEvent } from '@angular/material/paginator';
import { Title } from '@angular/platform-browser';
import { Sort } from '@angular/material/sort';

import { M } from '@allors/default/workspace/meta';
import { And, Equals } from '@allors/system/workspace/domain';
import {
  Priority,
  RequirementState,
  SerialisedItem,
  WorkRequirement,
} from '@allors/default/workspace/domain';
import {
  Action,
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
import { NavigationService } from '@allors/base/workspace/angular/application';
import {
  DeleteActionService,
  EditActionService,
  SorterService,
} from '@allors/base/workspace/angular-material/application';
import { ContextService } from '@allors/base/workspace/angular/foundation';
import { formatDistance } from 'date-fns';
import { CustomerOrganisationId } from '../../../services/state/customer-organisation-id';

interface Row extends TableRow {
  object: WorkRequirement;
  number: string;
  state: string;
  priority: string;
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
    public editRoleService: EditActionService,
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
        { name: 'number', sort: true },
        { name: 'state', sort: true },
        { name: 'priority', sort: true },
        { name: 'equipment', sort: true },
        { name: 'location', sort: true },
        { name: 'fleetCode', sort: true },
        { name: 'lastModifiedDate', sort: true },
      ],
      actions: [this.edit, this.delete],
      defaultAction: this.edit,
      pageSize: 50,
      initialSort: 'number',
      initialSortDirection: 'desc',
    });
  }

  public ngOnInit(): void {
    const m = this.m;
    const { pullBuilder: pull } = m;
    const x = {};

    const requirementStateSearch = new SearchFactory({
      objectType: m.RequirementState,
      roleTypes: [m.RequirementState.Name],
    });

    const prioritySearch = new SearchFactory({
      objectType: m.Priority,
      predicates: [
        { kind: 'Equals', propertyType: m.Enumeration.IsActive, value: true },
      ],
      roleTypes: [m.Priority.Name],
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
            propertyType: m.WorkRequirement.RequirementState,
            parameter: 'state',
          },
          {
            kind: 'Equals',
            propertyType: m.WorkRequirement.Priority,
            parameter: 'priority',
          },
          {
            kind: 'Like',
            roleType: m.WorkRequirement.RequirementNumber,
            parameter: 'Number',
          },
          {
            kind: 'Like',
            roleType: m.WorkRequirement.Location,
            parameter: 'Location',
          },
          {
            kind: 'Equals',
            propertyType: m.WorkRequirement.FixedAsset,
            parameter: 'equipment',
          },
          {
            kind: 'ContainedIn',
            propertyType: m.WorkRequirement.FixedAsset,
            extent: {
              kind: 'Filter',
              objectType: m.SerialisedItem,
              predicate: {
                kind: 'Like',
                roleType: m.SerialisedItem.CustomerReferenceNumber,
                parameter: 'fleetcode',
              },
            },
          },
        ],
      },
      {
        state: {
          search: () => requirementStateSearch,
          display: (v: RequirementState) => v && v.Name,
        },
        priority: {
          search: () => prioritySearch,
          display: (v: Priority) => v && v.Name,
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
      propertyType: m.WorkRequirement.Originator,
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
