import { Component } from '@angular/core';

import {
  RefreshService,
  SharedPullService,
} from '@allors/base/workspace/angular/foundation';

import { WorkspaceService } from '@allors/base/workspace/angular/foundation';
import {
  AllorsViewSummaryPanelComponent,
  NavigationService,
  PanelService,
  ScopedService,
} from '@allors/base/workspace/angular/application';
import { IPullResult, Pull } from '@allors/system/workspace/domain';
import { M } from '@allors/default/workspace/meta';
import { Vehicle } from '@allors/default/workspace/domain';

@Component({
  selector: 'vehicle-summary-panel',
  templateUrl: './vehicle-summary-panel.component.html',
})
export class VehicleSummaryPanelComponent extends AllorsViewSummaryPanelComponent {
  m: M;

  object: Vehicle;

  constructor(
    scopedService: ScopedService,
    panelService: PanelService,
    refreshService: RefreshService,
    sharedPullService: SharedPullService,
    workspaceService: WorkspaceService,
    public navigation: NavigationService
  ) {
    super(scopedService, panelService, sharedPullService, refreshService);
    this.m = workspaceService.workspace.configuration.metaPopulation as M;
  }

  onPreSharedPull(pulls: Pull[], prefix?: string) {
    const { m } = this;
    const { pullBuilder: p } = m;

    const id = this.scoped.id;

    pulls.push(
      p.Vehicle({
        name: prefix,
        objectId: id,
      })
    );
  }

  onPostSharedPull(loaded: IPullResult, prefix?: string) {
    this.object = loaded.object<Vehicle>(prefix);
  }
}
