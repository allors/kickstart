import { M } from '@allors/default/workspace/meta';
import { IObject, IRule } from '@allors/system/workspace/domain';
import {
  IataGseCodeDisplayNameRule,
  OperatingHoursTransactionDaysRule,
  OperatingHoursTransactionDeltaRule,
} from '@allors/aviation-extranet/workspace/derivations';

export function ruleBuilder(m: M): IRule<IObject>[] {
  return [
    new IataGseCodeDisplayNameRule(m),
    new OperatingHoursTransactionDaysRule(m),
    new OperatingHoursTransactionDeltaRule(m),
  ];
}
