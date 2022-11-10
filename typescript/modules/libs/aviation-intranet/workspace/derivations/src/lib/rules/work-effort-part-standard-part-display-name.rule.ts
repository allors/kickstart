import { IRule } from '@allors/system/workspace/domain';
import { Composite, Dependency, RoleType } from '@allors/system/workspace/meta';
import { M } from '@allors/default/workspace/meta';
import { WorkEffortPartStandard } from '@allors/default/workspace/domain';

export class WorkEffortPartStandardPartDisplayNameRule
  implements IRule<WorkEffortPartStandard>
{
  objectType: Composite;
  roleType: RoleType;
  dependencies: Dependency[];
  m: M;

  constructor(m: M) {
    this.m = m;

    this.objectType = m.WorkEffortPartStandard;
    this.roleType = m.WorkEffortPartStandard.PartDisplayName;
  }

  derive(match: WorkEffortPartStandard) {
    return match.Part?.DisplayName;
  }
}
