import { IRule } from '@allors/system/workspace/domain';
import { Composite, Dependency, RoleType } from '@allors/system/workspace/meta';
import { M } from '@allors/default/workspace/meta';
import { NonSerialisedInventoryItem } from '@allors/default/workspace/domain';

export class NonSerialisedInventoryItemSpanishPartDisplayNameRule
  implements IRule<NonSerialisedInventoryItem>
{
  objectType: Composite;
  roleType: RoleType;
  dependencies: Dependency[];

  m: M;

  constructor(m: M) {
    this.m = m;
    const { dependency: d } = this.m;

    this.objectType = m.NonSerialisedInventoryItem;
    this.roleType = m.NonSerialisedInventoryItem.SpanishPartDisplayName;

    this.dependencies = [
      d(m.NonSerialisedInventoryItem, (v) => v.Part),
      d(m.Part, (v) => v.SupplierOfferingsWherePart),
      d(m.SupplierOffering, (v) => v.Supplier),
    ];
  }

  derive(nonSerialisedInventoryItem: NonSerialisedInventoryItem) {
    const supplierInfo =
      nonSerialisedInventoryItem.Part?.SupplierOfferingsWherePart.map(
        (v) => `${v.Supplier?.DisplayName}: (${v.SupplierProductId})`
      ).join(', ');
    return `${nonSerialisedInventoryItem.FacilityName} ${
      nonSerialisedInventoryItem?.Part?.SpanishName ??
      nonSerialisedInventoryItem?.Part?.DisplayName
    }, ${supplierInfo}, QOH: ${nonSerialisedInventoryItem?.QuantityOnHand}`;
  }
}
