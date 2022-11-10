import { isBefore, isAfter } from 'date-fns';
import { IRule } from '@allors/system/workspace/domain';
import { Composite, Dependency, RoleType } from '@allors/system/workspace/meta';
import { M } from '@allors/default/workspace/meta';
import { OrderAdjustment, QuoteItem } from '@allors/default/workspace/domain';

export class QuoteItemUnitVatRule implements IRule<QuoteItem> {
  objectType: Composite;
  roleType: RoleType;
  dependencies: Dependency[];

  constructor(m: M) {
    const { dependency: d } = m;

    this.objectType = m.QuoteItem;
    this.roleType = m.QuoteItem.UnitVat;
  }

  derive(match: QuoteItem) {
    const quote = match.QuoteWhereQuoteItem;
    let unitBasePrice = 0;
    let unitDiscount = 0;
    let unitSurcharge = 0;

    const vatRegime = match.AssignedVatRegime ?? quote?.DerivedVatRegime;
    const vatRate = vatRegime?.VatRates.find(
      (v) =>
        isBefore(new Date(v.FromDate), quote.IssueDate) &&
        (v.ThroughDate == null ||
          isAfter(new Date(v.ThroughDate), quote.IssueDate))
    );

    if (match.AssignedUnitPrice != null) {
      unitBasePrice = parseFloat(match.AssignedUnitPrice);
    }

    if (unitBasePrice > 0) {
      match.DiscountAdjustments.forEach((v: OrderAdjustment) => {
        unitDiscount +=
          v.Percentage != null
            ? (unitBasePrice * parseFloat(v.Percentage)) / 100
            : parseFloat(v.Amount ?? '0');
      });

      match.SurchargeAdjustments.forEach((v: OrderAdjustment) => {
        unitSurcharge +=
          v.Percentage != null
            ? (unitBasePrice * parseFloat(v.Percentage)) / 100
            : parseFloat(v.Amount ?? '0');
      });

      const unitPrice = unitBasePrice - unitDiscount + unitSurcharge;

      return ((unitPrice * parseFloat(vatRate?.Rate ?? '0')) / 100).toFixed(2);
    }

    return '0';
  }
}