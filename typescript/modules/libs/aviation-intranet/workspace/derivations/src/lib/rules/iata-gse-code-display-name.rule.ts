import { IRule } from '@allors/system/workspace/domain';
import { Composite, Dependency, RoleType } from '@allors/system/workspace/meta';
import { M } from '@allors/default/workspace/meta';
import { IataGseCode } from '@allors/default/workspace/domain';

export class IataGseCodeDisplayNameRule implements IRule<IataGseCode> {
  objectType: Composite;
  roleType: RoleType;
  dependencies: Dependency[];

  constructor(m: M) {
    this.objectType = m.IataGseCode;
    this.roleType = m.IataGseCode.DisplayName;
  }

  derive(iataGseCode: IataGseCode) {
    return `${iataGseCode.Code}: ${iataGseCode.Name}`;
  }
}
