import { IRule } from '@allors/system/workspace/domain';
import { Composite, Dependency, RoleType } from '@allors/system/workspace/meta';
import { M } from '@allors/default/workspace/meta';
import { SerialisedItem, UnifiedGood } from '@allors/default/workspace/domain';

export class SerialisedItemMarketValueRule implements IRule<SerialisedItem> {
  objectType: Composite;
  roleType: RoleType;
  dependencies: Dependency[];

  constructor(m: M) {
    const { dependency: d } = m;

    this.objectType = m.SerialisedItem;
    this.roleType = m.SerialisedItem.MarketValue;

    this.dependencies = [d(m.SerialisedItem, (v) => v.PartWhereSerialisedItem)];
  }

  derive(match: SerialisedItem) {
    const good = match.PartWhereSerialisedItem as UnifiedGood | null;

    if (
      match.canReadPurchasePrice &&
      match.ManufacturingYear != null &&
      good?.ReplacementValue != null &&
      good.LifeTime != null
    ) {
      return Math.round(
        parseFloat(good.ReplacementValue) *
          Math.exp((-2.045 * match.Age) / good.LifeTime)
      ).toFixed(2);
    }

    return '0';
  }
}
