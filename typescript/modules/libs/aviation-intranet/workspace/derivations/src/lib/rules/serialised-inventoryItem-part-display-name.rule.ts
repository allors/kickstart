import { IRule } from '@allors/system/workspace/domain';
import { Composite, Dependency, RoleType } from '@allors/system/workspace/meta';
import { M } from '@allors/default/workspace/meta';
import { SerialisedInventoryItem } from '@allors/default/workspace/domain';

export class SerialisedInventoryItemDisplayNameRule
  implements IRule<SerialisedInventoryItem>
{
  objectType: Composite;
  roleType: RoleType;
  dependencies: Dependency[];

  constructor(m: M) {
    const { dependency: d } = m;

    this.objectType = m.SerialisedInventoryItem;
    this.roleType = m.SerialisedInventoryItem.PartDisplayName;

    this.dependencies = [
      d(m.SerialisedInventoryItem, (v) => v.Part),
      d(m.Part, (v) => v.SupplierOfferingsWherePart),
    ];
  }

  derive(match: SerialisedInventoryItem) {
    const supplierInfo = match.Part?.SupplierOfferingsWherePart.map(
      (v) => `${v.Supplier.DisplayName}: (${v.SupplierProductId})`
    ).join(', ');
    return `${match.Part?.DisplayName}, ${supplierInfo}`;
  }
}
