import { Component } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';

import {
  ErrorService,
  InvokeService,
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
import { WorkEffortType } from '@allors/default/workspace/domain';

@Component({
  // tslint:disable-next-line:component-selector
  selector: 'workefforttype-summary-panel',
  templateUrl: './workefforttype-summary-panel.component.html',
})
export class WorkEffortTypeSummaryPanelComponent extends AllorsViewSummaryPanelComponent {
  m: M;

  workEffortType: WorkEffortType;

  constructor(
    scopedService: ScopedService,
    panelService: PanelService,
    refreshService: RefreshService,
    sharedPullService: SharedPullService,
    workspaceService: WorkspaceService,
    private snackBar: MatSnackBar,
    private invokeService: InvokeService,
    private errorService: ErrorService,
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
      p.WorkEffortType({
        name: prefix,
        objectId: id,
        include: {
          UnifiedGood: {},
          ProductCategory: {},
        },
      })
    );
  }

  onPostSharedPull(loaded: IPullResult, prefix?: string) {
    this.workEffortType = loaded.object<WorkEffortType>(prefix);
  }
}
