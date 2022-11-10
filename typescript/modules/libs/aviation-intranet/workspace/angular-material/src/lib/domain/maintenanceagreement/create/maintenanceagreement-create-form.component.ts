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
import {
  FetcherService,
  Filters,
  InternalOrganisationId,
} from '@allors/apps-intranet/workspace/angular-material';
import { CustomFilters } from '../../../filters/filters';
import { isAfter, isBefore, isSameDay } from 'date-fns';

@Component({
  templateUrl: './maintenanceagreement-create-form.component.html',
  providers: [ContextService],
})
export class MaintenanceAgreementCreateFormComponent extends AllorsFormComponent<MaintenanceAgreement> {
  public m: M;

  customerRelationshipsFilter: SearchFactory;
  serialisedgoodsFilter: SearchFactory;
  workEffortTypes: WorkEffortType[];

  constructor(
    @Self() public allors: ContextService,
    errorService: ErrorService,
    form: NgForm,
    private internalOrganisationId: InternalOrganisationId,
    private fetcher: FetcherService
  ) {
    super(allors, errorService, form);
    this.m = allors.metaPopulation as M;
    const { treeBuilder } = this.m;

    this.serialisedgoodsFilter = Filters.serialisedgoodsFilter(this.m);
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

    this.onPrePullInitialize(pulls);
  }

  onPostPull(pullResult: IPullResult) {
    this.object = this.context.create(this.createRequest.objectType);
    this.fromDateSelected(new Date());

    this.onPostPullInitialize(pullResult);
  }

  public fromDateSelected(fromDate: any): void {
    const m = this.m;
    const { pullBuilder: pull } = m;
    const x = {};

    const pulls = [
      pull.WorkEffortType({
        sorting: [{ roleType: m.WorkEffortType.DisplayName }],
      }),
    ];

    this.allors.context.pull(pulls).subscribe((loaded) => {
      const allWorkEffortTypes = loaded.collection<WorkEffortType>(
        m.WorkEffortType
      );
      this.workEffortTypes = allWorkEffortTypes?.filter((v) => {
        const before = isBefore(new Date(v.FromDate), new Date(fromDate));
        const sameDay = isSameDay(new Date(v.FromDate), new Date(fromDate));
        const after =
          v.ThroughDate == null ||
          isAfter(new Date(v.ThroughDate), new Date(fromDate));
        return (before || sameDay) && after;
      });
    });
  }
}
