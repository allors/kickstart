import { isBefore, isAfter } from 'date-fns';
import { IRule } from '@allors/system/workspace/domain';
import { Composite, Dependency, RoleType } from '@allors/system/workspace/meta';
import { M } from '@allors/default/workspace/meta';
import {
  OrderAdjustment,
  SalesOrderItem,
} from '@allors/default/workspace/domain';

export class SalesOrderItemTotalIncVatRule implements IRule<SalesOrderItem> {
  objectType: Composite;
  roleType: RoleType;
  dependencies: Dependency[];

  constructor(m: M) {
    const { dependency: d } = m;

    this.objectType = m.SalesOrderItem;
    this.roleType = m.SalesOrderItem.TotalIncVat;
  }

  derive(match: SalesOrderItem) {
    const order = match.SalesOrderWhereSalesOrderItem;
    let unitBasePrice = 0;
    let unitDiscount = 0;
    let unitSurcharge = 0;

    const vatRegime = match.AssignedVatRegime ?? order?.DerivedVatRegime;
    const vatRate = vatRegime?.VatRates.find(
      (v) =>
        isBefore(new Date(v.FromDate), order.OrderDate) &&
        (v.ThroughDate == null ||
          isAfter(new Date(v.ThroughDate), order.OrderDate))
    );

    if (match.AssignedUnitPrice != null) {
      unitBasePrice = parseFloat(match.AssignedUnitPrice);
    }

    let unitVat = 0;
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

      unitVat = (unitBasePrice * parseFloat(vatRate?.Rate ?? '0')) / 100;
    }

    const unitPrice = unitBasePrice - unitDiscount + unitSurcharge;
    const totalExVat = unitPrice * parseFloat(match.QuantityOrdered ?? '0');
    const totalVat = unitVat * parseFloat(match.QuantityOrdered ?? '0');

    return (totalExVat + totalVat).toFixed(2);
  }
}
