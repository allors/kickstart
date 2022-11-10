import { IRule } from '@allors/system/workspace/domain';
import { Composite, Dependency, RoleType } from '@allors/system/workspace/meta';
import { M } from '@allors/default/workspace/meta';
import { SerialisedItem } from '@allors/default/workspace/domain';

export class SerialisedItemGrossBookValueRule implements IRule<SerialisedItem> {
  objectType: Composite;
  roleType: RoleType;
  dependencies: Dependency[];

  constructor(m: M) {
    this.objectType = m.SerialisedItem;
    this.roleType = m.SerialisedItem.GrossBookValue;
  }

  derive(match: SerialisedItem) {
    const transportCost = match.ActualTransportCost;
    const refurbishCost = match.ActualRefurbishCost;
    const otherCost = match.ActualOtherCost;
    if (match.canReadPurchasePrice && match.PurchasePrice != null) {
      let grossBookValue = parseFloat(match.PurchasePrice);

      if (transportCost) {
        grossBookValue += parseFloat(transportCost);
      }

      if (refurbishCost) {
        grossBookValue += parseFloat(refurbishCost);
      }

      if (otherCost) {
        grossBookValue += parseFloat(otherCost);
      }

      return grossBookValue.toFixed(2);
    }

    return '0';
  }
}
