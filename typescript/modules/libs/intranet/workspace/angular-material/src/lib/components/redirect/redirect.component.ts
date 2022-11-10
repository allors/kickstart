import { OnInit, Component, Self } from '@angular/core';
import { Router } from '@angular/router';
import {
  ContextService,
  UserId,
} from '@allors/base/workspace/angular/foundation';
import { User } from '@allors/default/workspace/domain';
import { M } from '@allors/default/workspace/meta';

@Component({
  template: '',
  providers: [ContextService],
})
export class RedirectComponent implements OnInit {
  constructor(
    @Self() public allors: ContextService,
    private router: Router,
    private userIdState: UserId
  ) {}

  public ngOnInit(): void {
    const m = this.allors.context.configuration.metaPopulation as M;
    const { pullBuilder: pull } = m;
    const x = {};

    const pulls = [
      pull.User({
        objectId: this.userIdState.value,
        include: {
          UserGroupsWhereMember: {
            InternalOrganisationWhereBlueCollarWorkerUserGroup: x,
          },
        },
      }),
    ];

    this.allors.context.pull(pulls).subscribe((loaded) => {
      const user = loaded.object<User>(m.User);
      const userGroups = user.UserGroupsWhereMember;

      const isWorker = !!userGroups.find(
        (v) => !!v.InternalOrganisationWhereBlueCollarWorkerUserGroup
      );
      const isStockManager = !!userGroups.find(
        (v) => !!v.InternalOrganisationWhereStockManagerUserGroup
      );

      if (isWorker) {
        this.router.navigate(['/shopfloor']);
      } else {
        this.router.navigate(['/dashboard']);
      }
    });
  }
}
