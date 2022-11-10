import { Component, Self } from '@angular/core';
import { NgForm } from '@angular/forms';

import { Pull, IPullResult } from '@allors/system/workspace/domain';
import {
  WorkRequirement,
  SerialisedItem,
  Priority,
  Person,
  Party,
  Organisation,
} from '@allors/default/workspace/domain';
import { M } from '@allors/default/workspace/meta';
import {
  ErrorService,
  AllorsFormComponent,
  UserId,
} from '@allors/base/workspace/angular/foundation';
import { ContextService } from '@allors/base/workspace/angular/foundation';
import { RadioGroupOption } from '@allors/base/workspace/angular-material/foundation';
import { isAfter, isBefore } from 'date-fns';
import { CustomerOrganisationId } from '../../../services/state/customer-organisation-id';
import { FetcherService } from '../../../services/fetcher/fetcher-service';

@Component({
  // tslint:disable-next-line:component-selector
  templateUrl: './workrequirement-form.component.html',
  providers: [ContextService],
})
export class WorkRequirementFormComponent extends AllorsFormComponent<WorkRequirement> {
  readonly m: M;
  public title: string;

  serialisedItems: SerialisedItem[];
  priorities: Priority[];
  priorityOptions: RadioGroupOption[];
  user: Person;
  customer: Party;
  selectedGse: any;
  serialisedItem: any;

  constructor(
    @Self() public allors: ContextService,
    errorService: ErrorService,
    form: NgForm,
    private userId: UserId,
    private customerOrganisationId: CustomerOrganisationId,
    private fetcher: FetcherService
  ) {
    super(allors, errorService, form);
    this.m = allors.metaPopulation as M;
  }

  onPrePull(pulls: Pull[]): void {
    const { m } = this;
    const { pullBuilder: p } = m;

    pulls.push(
      p.Person({
        objectId: this.userId.value,
        include: {
          CurrentOrganisationContactRelationships: {
            Organisation: {},
          },
        },
      }),
      p.Party({
        objectId: this.customerOrganisationId.value,
        include: {
          PartyContactMechanismsWhereParty: {},
          CurrentContacts: {},
          CustomerRelationshipsWhereCustomer: {
            InternalOrganisation: {},
          },
          CurrentPartyContactMechanisms: {
            ContactMechanism: {
              PostalAddress_Country: {},
            },
          },
        },
      }),
      p.Priority({
        predicate: {
          kind: 'Equals',
          propertyType: m.Priority.IsActive,
          value: true,
        },
        sorting: [{ roleType: m.Priority.DisplayOrder }],
      }),
      p.SerialisedItem({
        predicate: {
          kind: 'And',
          operands: [
            {
              kind: 'Or',
              operands: [
                {
                  kind: 'Equals',
                  propertyType: m.SerialisedItem.OwnedBy,
                  value: this.customerOrganisationId.value,
                },
                {
                  kind: 'Equals',
                  propertyType: m.SerialisedItem.RentedBy,
                  value: this.customerOrganisationId.value,
                },
              ],
            },
          ],
        },
        sorting: [{ roleType: m.SerialisedItem.DisplayName }],
      })
    );

    if (this.editRequest) {
      pulls.push(
        p.WorkRequirement({
          name: '_object',
          objectId: this.editRequest.objectId,
          include: {
            FixedAsset: {},
            LastModifiedBy: {},
            Pictures: {},
          },
        })
      );
    }

    const initializer = this.createRequest?.initializer;
    if (initializer) {
      pulls.push(
        p.SerialisedItem({
          objectId: initializer.id,
          include: {
            OwnedBy: {},
            RentedBy: {},
          },
        })
      );
    }
  }

  onPostPull(pullResult: IPullResult) {
    this.object = this.editRequest
      ? pullResult.object('_object')
      : this.context.create(this.createRequest.objectType);
    this.user = pullResult.object<Person>(this.m.Person);
    this.customer = pullResult.object<Party>(this.m.Party);
    this.serialisedItem = pullResult.object<SerialisedItem>(
      this.m.SerialisedItem
    );
    this.serialisedItems = pullResult.collection<SerialisedItem>(
      this.m.SerialisedItem
    );
    this.priorities = pullResult.collection<Priority>(this.m.Priority);
    this.priorityOptions = this.priorities.map((v) => {
      return {
        label: v.Name,
        value: v,
      };
    });

    if (this.createRequest) {
      this.object.Originator = this.customer;

      if (this.serialisedItem !== undefined) {
        if (
          this.serialisedItem.OwnedBy != null &&
          !(<Organisation>this.serialisedItem.OwnedBy).IsInternalOrganisation
        ) {
          this.object.Originator = this.serialisedItem.OwnedBy;
        } else if (
          this.serialisedItem.RentedBy != null &&
          !(<Organisation>this.serialisedItem.RentedBy).IsInternalOrganisation
        ) {
          this.object.Originator = this.serialisedItem.RentedBy;
        }

        this.object.FixedAsset = this.serialisedItem;
      }

      const internalOrganisations =
        this.customer.CustomerRelationshipsWhereCustomer.filter(
          (v) =>
            isBefore(new Date(v.FromDate), new Date()) &&
            (!v.ThroughDate || isAfter(new Date(v.ThroughDate), new Date()))
        ).map((v) => v.InternalOrganisation);

      const nl = internalOrganisations?.find(
        (v: Organisation) => v.Name === 'AVIACO NEDERLAND B.V.'
      );
      const es = internalOrganisations?.find(
        (v: Organisation) => v.Name === 'AVIATION REPAIRS AND MAINTENANCE, S.L.'
      );

      if (nl != null) {
        this.object.ServicedBy = nl as Organisation;
      } else if (es != null) {
        this.object.ServicedBy = es as Organisation;
      } else {
        this.object.ServicedBy = internalOrganisations[0] as Organisation;
      }
    }
  }

  public override save(): void {
    this.onSave();
    super.save();
  }

  private onSave() {
    const gse = this.object.FixedAsset as SerialisedItem;
    this.object.Description = `Service request for: ${gse?.DisplayName}`;
  }
}
