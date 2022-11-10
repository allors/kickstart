import { IRule } from '@allors/system/workspace/domain';
import { Composite, Dependency, RoleType } from '@allors/system/workspace/meta';
import { M } from '@allors/default/workspace/meta';
import { NonUnifiedPart, Settings } from '@allors/default/workspace/domain';

export class NonUnifiedPartExternalSellingPriceRule
  implements IRule<NonUnifiedPart>
{
  objectType: Composite;
  roleType: RoleType;
  dependencies: Dependency[];

  constructor(private m: M) {
    this.objectType = m.NonUnifiedPart;
    this.roleType = m.NonUnifiedPart.SuggestedExternalSellingPrice;
  }

  derive(nonUnifiedPart: NonUnifiedPart) {
    const session = nonUnifiedPart.strategy.session;
    const settings = session.instantiate<Settings>(this.m.Settings)[0];
    const maxPurchasePrice = Math.max(
      ...nonUnifiedPart.SupplierOfferingsWherePart.map((v) => {
        return parseFloat(v.Price);
      })
    );

    const sellingPrice =
      Math.round(
        maxPurchasePrice *
          (1 + parseFloat(settings.PartSurchargePercentage) / 100) *
          100
      ) / 100;

    return sellingPrice;
  }
}
