import { Component, Self } from '@angular/core';
import { NgForm } from '@angular/forms';

import { Pull, IPullResult } from '@allors/system/workspace/domain';
import {
  MaintenanceAgreement,
  WorkEffortType,
} from '@allors/default/workspace/domain';
import { M } from '@allors/default/workspace/meta';
import {
  ErrorService,
  AllorsFormComponent,
  SearchFactory,
} from '@allors/base/workspace/angular/foundation';
import { ContextService } from '@allors/base/workspace/angular/foundation';
import { InternalOrganisationId } from '@allors/apps-intranet/workspace/angular-material';
import { CustomFilters } from '../../../filters/filters';

@Component({
  // tslint:disable-next-line:component-selector
  selector: 'maintenanceagreement-edit-form',
  templateUrl: './maintenanceagreement-edit-form.component.html',
  providers: [ContextService],
})
export class MaintenanceAgreementEditFormComponent extends AllorsFormComponent<MaintenanceAgreement> {
  readonly m: M;
  customerRelationshipsFilter: SearchFactory;

  workEffortTypes: WorkEffortType[];

  constructor(
    @Self() public allors: ContextService,
    errorService: ErrorService,
    form: NgForm,
    private internalOrganisationId: InternalOrganisationId
  ) {
    super(allors, errorService, form);
    this.m = allors.metaPopulation as M;
    const { treeBuilder } = this.m;

    this.customerRelationshipsFilter =
      CustomFilters.customerRelationshipsFilter(
        this.m,
        treeBuilder,
        internalOrganisationId.value
      );
  }

  onPrePull(pulls: Pull[]): void {
    const { m } = this;
    const { pullBuilder: p } = m;

    pulls.push(
      p.WorkEffortType({
        sorting: [{ roleType: m.WorkEffortType.DisplayName }],
      })
    );

    if (this.editRequest) {
      pulls.push(
        p.MaintenanceAgreement({
          name: '_object',
          objectId: this.editRequest.objectId,
          include: {
            WorkEffortType: {},
            PartyRelationshipWhereAgreement: {},
          },
        })
      );
    }

    this.onPrePullInitialize(pulls);
  }

  onPostPull(pullResult: IPullResult) {
    this.object = pullResult.object('_object');

    this.onPostPullInitialize(pullResult);

    this.workEffortTypes = pullResult.collection<WorkEffortType>(
      this.m.WorkEffortType
    );
  }
}
