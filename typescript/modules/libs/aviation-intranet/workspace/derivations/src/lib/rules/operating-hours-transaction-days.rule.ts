import { differenceInCalendarDays } from 'date-fns';
import { IRule } from '@allors/system/workspace/domain';
import { Composite, Dependency, RoleType } from '@allors/system/workspace/meta';
import { M } from '@allors/default/workspace/meta';
import { OperatingHoursTransaction } from '@allors/default/workspace/domain';

export class OperatingHoursTransactionDaysRule
  implements IRule<OperatingHoursTransaction>
{
  objectType: Composite;
  roleType: RoleType;
  dependencies: Dependency[];

  constructor(m: M) {
    const { dependency: d } = m;

    this.objectType = m.OperatingHoursTransaction;
    this.roleType = m.OperatingHoursTransaction.Days;
    this.dependencies = [
      d(m.SerialisedItem, (v) => v.SyncedOperatingHoursTransactions),
    ];
  }

  derive(transaction: OperatingHoursTransaction) {
    if (transaction.PreviousTransaction != null) {
      return differenceInCalendarDays(
        transaction.RecordingDate,
        transaction.PreviousTransaction.RecordingDate
      );
    } else {
      return null;
    }
  }
}
