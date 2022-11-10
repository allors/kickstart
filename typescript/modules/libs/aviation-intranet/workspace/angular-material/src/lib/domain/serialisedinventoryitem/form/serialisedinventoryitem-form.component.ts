import { Component, Self } from '@angular/core';
import { NgForm } from '@angular/forms';

import { Pull, IPullResult } from '@allors/system/workspace/domain';
import {
  Facility,
  SerialisedInventoryItem,
  SerialisedItem,
  UnifiedGood,
} from '@allors/default/workspace/domain';
import { M } from '@allors/default/workspace/meta';
import {
  ErrorService,
  AllorsFormComponent,
} from '@allors/base/workspace/angular/foundation';
import { ContextService } from '@allors/base/workspace/angular/foundation';
import { FetcherService } from '@allors/apps-intranet/workspace/angular-material';

@Component({
  templateUrl: './serialisedinventoryitem-form.component.html',
  providers: [ContextService],
})
export class SerialisedInventoryItemFormComponent extends AllorsFormComponent<SerialisedInventoryItem> {
  public m: M;

  addFacility = false;
  facilities: Facility[];
  serialisedItems: SerialisedItem[];

  constructor(
    @Self() public allors: ContextService,
    errorService: ErrorService,
    form: NgForm,
    private fetcher: FetcherService
  ) {
    super(allors, errorService, form);
    this.m = allors.metaPopulation as M;
  }

  onPrePull(pulls: Pull[]): void {
    const { m } = this;
    const { pullBuilder: p } = m;

    pulls.push(p.Facility({ sorting: [{ roleType: m.Facility.Name }] }));

    if (this.editRequest) {
      pulls.push(
        p.SerialisedInventoryItem({
          name: '_object',
          objectId: this.editRequest.objectId,
          include: {
            SerialisedItem: {
              PartWhereSerialisedItem: {
                SerialisedItems: {},
              },
            },
          },
        })
      );
    }

    this.onPrePullInitialize(pulls);
  }

  onPostPull(pullResult: IPullResult) {
    this.object = this.editRequest
      ? pullResult.object('_object')
      : this.context.create(this.createRequest.objectType);

    this.facilities = pullResult.collection(this.m.Facility);

    this.onPostPullInitialize(pullResult);

    if (this.createRequest) {
      const part = pullResult.object<UnifiedGood>('_initializer');
      this.serialisedItems = part.SerialisedItems;
    } else {
      this.serialisedItems =
        this.object.SerialisedItem.PartWhereSerialisedItem.SerialisedItems;
    }
  }

  public facilityAdded(facility: Facility): void {
    this.facilities.push(facility);
    this.object.Facility = facility;
  }
}
