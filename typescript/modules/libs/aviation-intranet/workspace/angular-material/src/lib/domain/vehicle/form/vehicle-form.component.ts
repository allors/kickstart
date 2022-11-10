import { Component, Self } from '@angular/core';
import { NgForm } from '@angular/forms';

import { Pull, IPullResult } from '@allors/system/workspace/domain';
import { Vehicle } from '@allors/default/workspace/domain';
import { M } from '@allors/default/workspace/meta';
import {
  ErrorService,
  AllorsFormComponent,
  SearchFactory,
} from '@allors/base/workspace/angular/foundation';
import { ContextService } from '@allors/base/workspace/angular/foundation';
import {
  FetcherService,
  Filters,
} from '@allors/apps-intranet/workspace/angular-material';

@Component({
  templateUrl: './vehicle-form.component.html',
  providers: [ContextService],
})
export class VehicleFormComponent extends AllorsFormComponent<Vehicle> {
  m: M;
  partiesFilter: SearchFactory;

  constructor(
    @Self() public allors: ContextService,
    errorService: ErrorService,
    form: NgForm
  ) {
    super(allors, errorService, form);
    this.m = allors.metaPopulation as M;

    this.partiesFilter = Filters.partiesFilter(this.m);
  }

  onPrePull(pulls: Pull[]): void {
    const { m } = this;
    const { pullBuilder: p } = m;

    if (this.editRequest) {
      pulls.push(
        p.Vehicle({
          name: '_object',
          objectId: this.editRequest.objectId,
          include: {
            OwnedBy: {},
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

    this.onPostPullInitialize(pullResult);
  }
}
