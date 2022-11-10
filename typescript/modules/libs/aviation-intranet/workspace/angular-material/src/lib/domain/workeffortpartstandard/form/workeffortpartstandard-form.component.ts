import { Component, Self } from '@angular/core';
import { NgForm } from '@angular/forms';

import { Pull, IPullResult } from '@allors/system/workspace/domain';
import {
  WorkEffortPartStandard,
  WorkEffortType,
} from '@allors/default/workspace/domain';
import { M } from '@allors/default/workspace/meta';
import {
  ErrorService,
  AllorsFormComponent,
  SearchFactory,
} from '@allors/base/workspace/angular/foundation';
import { ContextService } from '@allors/base/workspace/angular/foundation';
import { Filters } from '@allors/apps-intranet/workspace/angular-material';

@Component({
  templateUrl: './workeffortpartstandard-form.component.html',
  providers: [ContextService],
})
export class WorkEffortPartStandardFormComponent extends AllorsFormComponent<WorkEffortPartStandard> {
  readonly m: M;

  nonUnifiedPartsFilter: SearchFactory;

  constructor(
    @Self() public allors: ContextService,
    errorService: ErrorService,
    form: NgForm
  ) {
    super(allors, errorService, form);
    this.m = allors.metaPopulation as M;

    this.nonUnifiedPartsFilter = Filters.nonUnifiedPartsFilter(this.m);
  }

  onPrePull(pulls: Pull[]): void {
    const { m } = this;
    const { pullBuilder: p } = m;

    if (this.editRequest) {
      pulls.push(
        p.WorkEffortPartStandard({
          name: '_object',
          objectId: this.editRequest.objectId,
          include: {
            Part: {},
          },
        })
      );
    }

    const initializer = this.createRequest?.initializer;
    if (initializer) {
      pulls.push(
        p.WorkEffortType({
          objectId: initializer.id,
        })
      );
    }
  }

  onPostPull(pullResult: IPullResult) {
    this.object = this.editRequest
      ? pullResult.object('_object')
      : this.context.create(this.createRequest.objectType);

    if (this.createRequest) {
      const workEffortType = pullResult.object<WorkEffortType>(
        this.m.WorkEffortType
      );

      workEffortType.addWorkEffortPartStandard(this.object);
    }
  }
}
