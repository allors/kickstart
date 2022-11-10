import { Component } from '@angular/core';

import {
  RefreshService,
  SharedPullService,
} from '@allors/base/workspace/angular/foundation';

import { WorkspaceService } from '@allors/base/workspace/angular/foundation';
import {
  AllorsViewSummaryPanelComponent,
  PanelService,
  ScopedService,
} from '@allors/base/workspace/angular/application';
import { IPullResult, Pull } from '@allors/system/workspace/domain';
import { M } from '@allors/default/workspace/meta';
import { MaintenanceAgreement } from '@allors/default/workspace/domain';

@Component({
  // tslint:disable-next-line:component-selector
  selector: 'maintenanceagreement-summary-panel',
  templateUrl: './maintenanceagreement-summary-panel.component.html',
})
export class MaintenanceAgreementSummaryPanelComponent extends AllorsViewSummaryPanelComponent {
  m: M;

  agreement: MaintenanceAgreement;

  constructor(
    scopedService: ScopedService,
    panelService: PanelService,
    refreshService: RefreshService,
    sharedPullService: SharedPullService,
    workspaceService: WorkspaceService
  ) {
    super(scopedService, panelService, sharedPullService, refreshService);
    this.m = workspaceService.workspace.configuration.metaPopulation as M;
  }

  onPreSharedPull(pulls: Pull[], prefix?: string) {
    const {
      m: { pullBuilder: p },
    } = this;

    const id = this.scoped.id;

    pulls.push(
      p.MaintenanceAgreement({
        name: prefix,
        objectId: id,
      })
    );
  }

  onPostSharedPull(loaded: IPullResult, prefix?: string) {
    this.agreement = loaded.object<MaintenanceAgreement>(prefix);
  }
}
