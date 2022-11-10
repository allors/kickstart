import { Component, Self } from '@angular/core';
import { NgForm } from '@angular/forms';

import { Pull, IPullResult, IObject } from '@allors/system/workspace/domain';
import {
  Facility,
  FacilityType,
  Organisation,
} from '@allors/default/workspace/domain';
import { M } from '@allors/default/workspace/meta';
import {
  ErrorService,
  AllorsFormComponent,
} from '@allors/base/workspace/angular/foundation';
import { ContextService } from '@allors/base/workspace/angular/foundation';

@Component({
  templateUrl: './facility-form.component.html',
  providers: [ContextService],
})
export class FacilityFormComponent extends AllorsFormComponent<Facility> {
  m: M;
  facilityTypes: FacilityType[];
  owners: Organisation[];
  warehouses: Facility[];
  notWorkshop: boolean;

  constructor(
    @Self() public allors: ContextService,
    errorService: ErrorService,
    form: NgForm
  ) {
    super(allors, errorService, form);
    this.m = allors.metaPopulation as M;
  }

  onPrePull(pulls: Pull[]): void {
    const { m } = this;
    const { pullBuilder: p } = m;

    pulls.push(
      p.FacilityType({
        predicate: {
          kind: 'Equals',
          propertyType: m.FacilityType.IsActive,
          value: true,
        },
        sorting: [{ roleType: m.FacilityType.Name }],
      }),
      p.FacilityType({
        name: 'warehouses',
        predicate: {
          kind: 'Equals',
          propertyType: m.FacilityType.UniqueId,
          value: 'd4a70252-58d0-425b-8f54-7f55ae01a7b3',
        },
        select: {
          FacilitiesWhereFacilityType: {},
        },
        sorting: [{ roleType: m.Facility.Name }],
      }),
      p.Organisation({
        predicate: {
          kind: 'Equals',
          propertyType: m.Organisation.IsInternalOrganisation,
          value: true,
        },
        sorting: [{ roleType: m.Organisation.DisplayName }],
      })
    );

    if (this.editRequest) {
      pulls.push(
        p.Facility({
          name: '_object',
          objectId: this.editRequest.objectId,
          include: {
            Owner: {},
            FacilityType: {},
            ParentFacility: {},
          },
        })
      );
    }

    this.onPrePullInitialize(pulls);
  }

  onPostPull(pullResult: IPullResult) {
    this.object = this.editRequest
      ? pullResult.object('_object')
      : this.context.create(this.createRequest.objectType);

    this.facilityTypes = pullResult.collection<FacilityType>(
      this.m.FacilityType
    );

    this.owners = pullResult.collection<Organisation>(this.m.Organisation);
    this.warehouses = pullResult.collection<Facility>('warehouses');

    this.onPostPullInitialize(pullResult);

    if (this.editRequest) {
      this.facilityTypeSelected(this.object.FacilityType);
    }
  }

  public facilityTypeSelected(facilityType: IObject) {
    const type = facilityType as FacilityType;
    this.notWorkshop = type.UniqueId !== '07d554f3-421b-47f3-915a-60b3639f7371';
  }
}
