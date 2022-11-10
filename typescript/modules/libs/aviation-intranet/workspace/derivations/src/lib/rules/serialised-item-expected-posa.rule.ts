import { IRule } from '@allors/system/workspace/domain';
import { Composite, Dependency, RoleType } from '@allors/system/workspace/meta';
import { M } from '@allors/default/workspace/meta';
import { SerialisedItem } from '@allors/default/workspace/domain';

export class SerialisedItemExpectedPosaRule implements IRule<SerialisedItem> {
  objectType: Composite;
  roleType: RoleType;
  dependencies: Dependency[];

  constructor(m: M) {
    this.objectType = m.SerialisedItem;
    this.roleType = m.SerialisedItem.ExpectedPosa;
  }

  derive(match: SerialisedItem) {
    if (match.canReadExpectedSalesPrice && match.ExpectedSalesPrice != null) {
      return (
        parseFloat(match.ExpectedSalesPrice) -
        parseFloat(match.ActualGrossBookValue)
      ).toFixed(2);
    }

    return '0';
  }
}
