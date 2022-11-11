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
import { Person, User } from '@allors/default/workspace/domain';

@Component({
  selector: 'person-summary-panel',
  templateUrl: './person-summary-panel.component.html',
})
export class PersonSummaryPanelComponent extends AllorsViewSummaryPanelComponent {
  m: M;

  person: Person;
  user: User;

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
    const {
      m: { pullBuilder: p },
    } = this;

    const id = this.scoped.id;

    pulls.push(
      p.Person({
        name: `${prefix}`,
        objectId: id,
        select: {
          TaskAssignmentsWhereUser: {},
        },
      })
    );
  }

  onPostSharedPull(loaded: IPullResult, prefix?: string) {
    this.person = loaded.object<Person>(prefix);
    this.user = loaded.object<User>(prefix);
  }
}
