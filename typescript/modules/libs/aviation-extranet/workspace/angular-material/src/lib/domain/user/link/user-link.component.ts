import { Component, OnInit } from '@angular/core';

import { Person } from '@allors/default/workspace/domain';
import {
  Action,
  RefreshService,
  SharedPullService,
  UserId,
  WorkspaceService,
} from '@allors/base/workspace/angular/foundation';
import { M } from '@allors/default/workspace/meta';
import {
  IPullResult,
  Pull,
  SharedPullHandler,
} from '@allors/system/workspace/domain';
import { EditActionService } from '@allors/base/workspace/angular-material/application';

@Component({
  // tslint:disable-next-line:component-selector
  selector: 'userprofile-link',
  templateUrl: './user-link.component.html',
})
export class UserLinkComponent implements SharedPullHandler, OnInit {
  edit: Action;

  user: Person;
  m: M;
  constructor(
    public sharedPullService: SharedPullService,
    public workspaceService: WorkspaceService,
    public refreshService: RefreshService,
    public editRoleService: EditActionService,
    private userId: UserId
  ) {
    this.edit = editRoleService.edit();

    this.m = this.workspaceService.metaPopulation as M;
    this.sharedPullService.register(this);
  }

  ngOnInit(): void {
    this.refreshService.refresh();
  }

  onPreSharedPull(pulls: Pull[], prefix: string): void {
    const {
      m: { pullBuilder: p },
    } = this;

    pulls.push(
      p.Person({
        name: prefix,
        objectId: this.userId.value,
        include: {
          UserProfile: {
            DefaultInternalOrganization: {},
          },
        },
      })
    );
  }

  onPostSharedPull(pullResult: IPullResult, prefix: string): void {
    this.user = pullResult.object<Person>(prefix);
  }

  toUser() {
    this.edit.execute(this.user);
  }
}
