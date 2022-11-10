import { Component, Self } from '@angular/core';
import { NgForm } from '@angular/forms';

import { Pull, IPullResult } from '@allors/system/workspace/domain';
import {
  Facility,
  InternalOrganisation,
  InventoryItem,
  Locale,
  NonSerialisedInventoryItem,
  NonSerialisedInventoryItemState,
  Part,
  Person,
  SerialisedInventoryItem,
  SerialisedInventoryItemState,
  WorkEffort,
  WorkEffortInventoryAssignment,
} from '@allors/default/workspace/domain';
import { M } from '@allors/default/workspace/meta';
import {
  ErrorService,
  AllorsFormComponent,
  SearchFactory,
  UserId,
} from '@allors/base/workspace/angular/foundation';
import { ContextService } from '@allors/base/workspace/angular/foundation';
import { FetcherService } from '@allors/apps-intranet/workspace/angular-material';

@Component({
  templateUrl: './workeffortinventoryassignment-form.component.html',
  providers: [ContextService],
})
export class WorkEffortInventoryAssignmentFormComponent extends AllorsFormComponent<WorkEffortInventoryAssignment> {
  readonly m: M;

  parts: Part[];
  workEffort: WorkEffort;
  inventoryItems: InventoryItem[];
  facility: Facility;
  state: NonSerialisedInventoryItemState | SerialisedInventoryItemState;
  serialised: boolean;
  facilities: Facility[];
  selectedFacility: Facility;
  internalOrganisation: InternalOrganisation;
  inventoryItemsFilter: SearchFactory;
  inventoryItem: InventoryItem;
  user: Person;
  dutchUser: boolean;
  spanishUser: boolean;

  constructor(
    @Self() public allors: ContextService,
    errorService: ErrorService,
    form: NgForm,
    private fetcher: FetcherService,
    private userId: UserId
  ) {
    super(allors, errorService, form);
    this.m = allors.metaPopulation as M;
  }

  onPrePull(pulls: Pull[]): void {
    const { m } = this;
    const { pullBuilder: p } = m;

    pulls.push(
      this.fetcher.internalOrganisation,
      this.fetcher.ownWarehouses,
      p.Locale({}),
      p.Person({
        objectId: this.userId.value,
        include: {
          UserProfile: {
            DefaulLocale: {},
          },
        },
      })
    );

    if (this.editRequest) {
      pulls.push(
        p.WorkEffortInventoryAssignment({
          name: '_object',
          objectId: this.editRequest.objectId,
          include: {
            Assignment: {},
            InventoryItem: {
              Part: {
                InventoryItemKind: {},
              },
            },
          },
        })
      );
    }

    const initializer = this.createRequest?.initializer;
    if (initializer) {
      pulls.push(
        p.WorkEffort({
          objectId: initializer.id,
        })
      );
    }
  }

  onPostPull(pullResult: IPullResult) {
    this.object = this.editRequest
      ? pullResult.object('_object')
      : this.context.create(this.createRequest.objectType);

    this.user = pullResult.object<Person>(this.m.Person);
    this.internalOrganisation =
      this.fetcher.getInternalOrganisation(pullResult);
    this.workEffort = pullResult.object<WorkEffort>(this.m.WorkEffort);
    this.facilities = this.fetcher.getOwnWarehouses(pullResult);

    const locales = pullResult.collection<Locale>(this.m.Locale);
    const dutchLocale = locales?.find((v) => v.Name === 'nl-NL');
    const spanishLocale = locales?.find((v) => v.Name === 'es-ES');

    this.dutchUser = this.user.UserProfile.DefaulLocale === dutchLocale;
    this.spanishUser = this.user.UserProfile.DefaulLocale === spanishLocale;

    if (this.createRequest) {
      this.object.Assignment = this.workEffort;

      this.selectedFacility = this.facilities?.find(
        (v) => v.Owner === this.internalOrganisation
      );
    } else {
      this.selectedFacility = this.facilities?.find(
        (v) => v.Owner === this.internalOrganisation
      );

      this.inventoryItemSelected(this.object.InventoryItem);
    }

    this.facilitySelected(this.selectedFacility);
  }

  public inventoryItemSelected(inventoryItem: any): void {
    const m = this.m;
    const { pullBuilder: pull } = m;
    const x = {};

    if (inventoryItem !== undefined) {
      const pulls = [
        pull.InventoryItem({
          object: inventoryItem,
          include: {
            Part: {
              InventoryItemKind: x,
            },
            Facility: x,
            SerialisedInventoryItem_SerialisedInventoryItemState: x,
            NonSerialisedInventoryItem_NonSerialisedInventoryItemState: x,
          },
        }),
      ];

      this.allors.context.pull(pulls).subscribe((pullResult) => {
        this.inventoryItem = pullResult.object<InventoryItem>(m.InventoryItem);

        this.serialised =
          this.inventoryItem.Part.InventoryItemKind.UniqueId ===
          '2596e2dd3f5d4588a4a2167d6fbe3fae';

        if (
          this.inventoryItem.strategy.cls === this.m.NonSerialisedInventoryItem
        ) {
          const item = inventoryItem as NonSerialisedInventoryItem;
          this.state = item.NonSerialisedInventoryItemState;
        } else {
          const item = inventoryItem as SerialisedInventoryItem;
          this.state = item.SerialisedInventoryItemState;
        }
      });
    }
  }

  public facilitySelected(facility: Facility): void {
    this.inventoryItemsFilter = new SearchFactory({
      objectType: this.m.NonSerialisedInventoryItem,
      predicates: [
        {
          kind: 'Equals',
          propertyType: this.m.NonSerialisedInventoryItem.Facility,
          object: facility,
        },
      ],
      roleTypes: [this.m.NonSerialisedInventoryItem.SearchString],
    });
  }
}
