import { format } from 'date-fns';
import { IRule } from '@allors/system/workspace/domain';
import { Composite, Dependency, RoleType } from '@allors/system/workspace/meta';
import { M } from '@allors/default/workspace/meta';
import { WorkEffortType } from '@allors/default/workspace/domain';

export class WorkEffortTypeDisplayNameRule implements IRule<WorkEffortType> {
  objectType: Composite;
  roleType: RoleType;
  dependencies: Dependency[];
  m: M;

  constructor(m: M) {
    this.m = m;

    this.objectType = m.WorkEffortType;
    this.roleType = m.WorkEffortType.DisplayName;
  }

  derive(match: WorkEffortType) {
    if (match.UnifiedGoodDisplayName) {
      let displayName = `${match.Name} for ${
        match.UnifiedGoodDisplayName
      } valid from ${format(new Date(match.FromDate), 'dd-MM-yyyy')}`;
      if (match.ThroughDate !== null) {
        displayName = `${displayName} through ${format(
          new Date(match.ThroughDate),
          'dd-MM-yyyy'
        )}`;
      }

      return displayName;
    }

    if (match.ProductCategoryDisplayName) {
      let displayName = `${match.Name} for ${
        match.ProductCategoryDisplayName
      } valid from ${format(new Date(match.FromDate), 'dd-MM-yyyy')}`;
      if (match.ThroughDate !== null) {
        displayName = `${displayName} through ${format(
          new Date(match.ThroughDate),
          'dd-MM-yyyy'
        )}`;
      }

      return displayName;
    }

    return '';
  }
}
