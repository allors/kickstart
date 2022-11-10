import { Composite, Dependency, RoleType } from '@allors/system/workspace/meta';
import { M } from '@allors/default/workspace/meta';
import { IRule } from '@allors/system/workspace/domain';
import { OperatingHoursTransaction } from '@allors/default/workspace/domain';

export class OperatingHoursTransactionDeltaRule
  implements IRule<OperatingHoursTransaction>
{
  objectType: Composite;
  roleType: RoleType;
  dependencies: Dependency[];

  constructor(m: M) {
    const { dependency: d } = m;

    this.objectType = m.OperatingHoursTransaction;
    this.roleType = m.OperatingHoursTransaction.Delta;
    this.dependencies = [
      d(m.SerialisedItem, (v) => v.SyncedOperatingHoursTransactions),
    ];
  }

  derive(transaction: OperatingHoursTransaction) {
    if (transaction.PreviousTransaction != null) {
      return (
        parseFloat(transaction.Value) -
        parseFloat(transaction.PreviousTransaction.Value)
      ).toString();
    } else {
      return null;
    }
  }
}
