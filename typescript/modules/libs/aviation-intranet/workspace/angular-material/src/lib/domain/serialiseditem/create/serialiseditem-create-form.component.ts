import { Component, Self } from '@angular/core';
import { NgForm } from '@angular/forms';

import { Pull, IPullResult } from '@allors/system/workspace/domain';
import {
  Enumeration,
  Facility,
  Locale,
  Organisation,
  Ownership,
  Part,
  Party,
  Person,
  SerialisedItem,
  SerialisedItemAvailability,
  SerialisedItemState,
} from '@allors/default/workspace/domain';
import { M } from '@allors/default/workspace/meta';
import {
  ErrorService,
  AllorsFormComponent,
  SearchFactory,
  UserId,
} from '@allors/base/workspace/angular/foundation';
import { ContextService } from '@allors/base/workspace/angular/foundation';
import {
  FetcherService,
  Filters,
  InternalOrganisationId,
} from '@allors/apps-intranet/workspace/angular-material';

@Component({
  templateUrl: './serialiseditem-create-form.component.html',
  providers: [ContextService],
})
export class SerialisedItemCreateFormComponent extends AllorsFormComponent<SerialisedItem> {
  readonly m: M;
  locales: Locale[];
  ownerships: Ownership[];
  organisations: Organisation[];
  organisationFilter: SearchFactory;
  serialisedItemStates: SerialisedItemState[];
  owner: Party;
  part: Part;
  itemPart: Part;
  selectedPart: Part;
  serialisedItemAvailabilities: Enumeration[];
  own: Ownership;
  thirdParty: Ownership;
  serialisedgoodsFilter: SearchFactory;
  partiesFilter: SearchFactory;

  constructor(
    @Self() public allors: ContextService,
    errorService: ErrorService,
    form: NgForm,
    private fetcher: FetcherService
  ) {
    super(allors, errorService, form);
    this.m = allors.metaPopulation as M;

    this.partiesFilter = Filters.partiesFilter(this.m);
    this.serialisedgoodsFilter = Filters.serialisedgoodsFilter(this.m);
  }

  onPrePull(pulls: Pull[]): void {
    const { m } = this;
    const { pullBuilder: p } = m;

    pulls.push(
      this.fetcher.internalOrganisation,
      this.fetcher.locales,
      p.Ownership({ sorting: [{ roleType: m.Ownership.Name }] }),
      p.SerialisedItemState({
        predicate: {
          kind: 'Equals',
          propertyType: m.SerialisedItemState.IsActive,
          value: true,
        },
        sorting: [{ roleType: m.SerialisedInventoryItemState.Name }],
      }),
      p.SerialisedItemAvailability({
        predicate: {
          kind: 'Equals',
          propertyType: m.SerialisedItemAvailability.IsActive,
          value: true,
        },
        sorting: [{ roleType: m.SerialisedItemAvailability.Name }],
      })
    );

    const initializer = this.createRequest?.initializer;
    if (initializer) {
      pulls.push(
        p.Party({ objectId: initializer.id }),
        p.Part({
          name: 'forPart',
          objectId: initializer.id,
          include: {
            SerialisedItems: {},
          },
        })
      );
    }
  }

  onPostPull(pullResult: IPullResult) {
    this.object = this.context.create(this.createRequest.objectType);

    const internalOrganisation =
      this.fetcher.getInternalOrganisation(pullResult);
    const externalOwner = pullResult.object<Party>(this.m.Party);
    this.owner = externalOwner || internalOrganisation;

    this.ownerships = pullResult.collection<Ownership>(this.m.Ownership);
    this.own = this.ownerships?.find(
      (v: Ownership) => v.UniqueId === '1cefe3e7-3f9a-43a1-b12c-93e8032d3880'
    );
    this.thirdParty = this.ownerships?.find(
      (v: Ownership) => v.UniqueId === '6b613409-bdf4-4a86-815f-6920d2fec8d3'
    );

    this.part = pullResult.object<Part>('forPart');

    this.serialisedItemStates = pullResult.collection<SerialisedItemState>(
      this.m.SerialisedItemState
    );
    this.serialisedItemAvailabilities =
      pullResult.collection<SerialisedItemAvailability>(
        this.m.SerialisedItemAvailability
      );

    this.ownerships = pullResult.collection<Ownership>(this.m.Ownership);

    this.locales = this.fetcher.getAdditionalLocales(pullResult);

    this.object.AvailableForSale = false;
    this.object.OwnedBy = this.owner;

    this.ownedBySelected(this.owner);

    if (this.part) {
      this.partSelected(this.part);
    }
  }

  public ownedBySelected(owner: any) {
    if (owner) {
      if (owner.IsInternalOrganisation) {
        this.object.Ownership = this.own;
      } else {
        this.object.Ownership = this.thirdParty;
      }
    }
  }

  public partSelected(part: any): void {
    if (part !== undefined) {
      this.selectedPart = part;

      const m = this.m;
      const { pullBuilder: pull } = m;
      const x = {};

      const pulls = [
        pull.Part({
          object: part,
          include: {
            SerialisedItems: x,
          },
        }),
      ];

      this.allors.context.pull(pulls).subscribe((pullResult) => {
        this.selectedPart = pullResult.object<Part>(m.Part);
      });
    } else {
      this.selectedPart = undefined;
    }
  }

  public override save(): void {
    this.selectedPart.addSerialisedItem(this.object);
    super.save();
  }
}
