import { IRule } from '@allors/system/workspace/domain';
import { Composite, Dependency, RoleType } from '@allors/system/workspace/meta';
import { M } from '@allors/default/workspace/meta';
import {
  CustomerRelationship,
  Vehicle,
} from '@allors/default/workspace/domain';

export class VehicleDisplayNameRule implements IRule<Vehicle> {
  objectType: Composite;
  roleType: RoleType;
  dependencies: Dependency[];

  m: M;

  constructor(m: M) {
    this.m = m;
    const { dependency: d } = this.m;

    this.objectType = m.Vehicle;
    this.roleType = m.Vehicle.DisplayName;

    this.dependencies = [];
  }

  derive(vehicle: Vehicle) {
    return `${vehicle.LicensePlateNumber}, ${vehicle.Make} ${vehicle.Model}`;
  }
}
