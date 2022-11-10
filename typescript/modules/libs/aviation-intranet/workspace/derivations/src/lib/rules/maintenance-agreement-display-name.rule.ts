import { IRule } from '@allors/system/workspace/domain';
import { Composite, Dependency, RoleType } from '@allors/system/workspace/meta';
import { M } from '@allors/default/workspace/meta';
import {
  CustomerRelationship,
  MaintenanceAgreement,
} from '@allors/default/workspace/domain';

export class MaintenanceAgreementDisplayNameRule
  implements IRule<MaintenanceAgreement>
{
  objectType: Composite;
  roleType: RoleType;
  dependencies: Dependency[];

  m: M;

  constructor(m: M) {
    this.m = m;
    const { dependency: d } = this.m;

    this.objectType = m.MaintenanceAgreement;
    this.roleType = m.MaintenanceAgreement.DisplayName;

    this.dependencies = [
      d(m.MaintenanceAgreement, (v) => v.WorkEffortType),
      d(m.MaintenanceAgreement, (v) => v.PartyRelationshipWhereAgreement),
    ];
  }

  derive(maintenanceAgreement: MaintenanceAgreement) {
    const customerRelationship =
      maintenanceAgreement.PartyRelationshipWhereAgreement as CustomerRelationship;
    return `${customerRelationship?.CustomerName} ' for ' ${maintenanceAgreement.WorkEffortType?.Name}`;
  }
}
