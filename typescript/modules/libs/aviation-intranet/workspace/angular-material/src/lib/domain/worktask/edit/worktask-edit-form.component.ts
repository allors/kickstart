import { Component, Self } from '@angular/core';
import { NgForm } from '@angular/forms';

import { Pull, IPullResult } from '@allors/system/workspace/domain';
import {
  Agreement,
  ContactMechanism,
  CustomerRelationship,
  Facility,
  MaintenanceAgreement,
  Organisation,
  OrganisationContactRelationship,
  Party,
  PartyContactMechanism,
  Person,
  Priority,
  SerialisedItem,
  WorkEffortFixedAssetAssignment,
  WorkEffortPurpose,
  WorkEffortState,
  WorkTask,
} from '@allors/default/workspace/domain';
import { M } from '@allors/default/workspace/meta';
import {
  ErrorService,
  AllorsFormComponent,
  SearchFactory,
} from '@allors/base/workspace/angular/foundation';
import { ContextService } from '@allors/base/workspace/angular/foundation';
import {
  FetcherService,
  Filters,
  InternalOrganisationId,
} from '@allors/apps-intranet/workspace/angular-material';
import { isAfter, isBefore, isSameDay } from 'date-fns';

@Component({
  // tslint:disable-next-line:component-selector
  selector: 'worktask-edit-form',
  templateUrl: './worktask-edit-form.component.html',
  providers: [ContextService],
})
export class WorkTaskEditFormComponent extends AllorsFormComponent<WorkTask> {
  readonly m: M;
  party: Party;
  workEffortStates: WorkEffortState[];
  priorities: Priority[];
  workEffortPurposes: WorkEffortPurpose[];
  employees: Person[];
  contactMechanisms: ContactMechanism[];
  contacts: Person[];
  addContactPerson = false;
  addContactMechanism: boolean;
  customersFilter: SearchFactory;
  subContractorsFilter: SearchFactory;
  agreements: Agreement[];
  workEffortFixedAssetAssignments: WorkEffortFixedAssetAssignment[];
  customerRelationship: CustomerRelationship;
  workshops: Facility[];

  constructor(
    @Self() public allors: ContextService,
    errorService: ErrorService,
    form: NgForm,
    private fetcher: FetcherService,
    private internalOrganisationId: InternalOrganisationId
  ) {
    super(allors, errorService, form);
    this.m = allors.metaPopulation as M;

    this.customersFilter = Filters.customersFilter(
      this.m,
      this.internalOrganisationId.value
    );
    this.subContractorsFilter = Filters.subContractorsFilter(
      this.m,
      this.internalOrganisationId.value
    );
  }

  onPrePull(pulls: Pull[]): void {
    const { m } = this;
    const { pullBuilder: p } = m;

    pulls.push(
      this.fetcher.internalOrganisation,
      p.InternalOrganisation({
        objectId: this.internalOrganisationId.value,
        include: {
          ActiveEmployees: {},
        },
      }),
      p.WorkTask({
        name: '_object',
        objectId: this.editRequest.objectId,
        include: {
          WorkEffortState: {},
          MaintenanceAgreement: {},
          FullfillContactMechanism: {},
          Priority: {},
          WorkEffortPurposes: {},
          PublicElectronicDocuments: {},
          PrivateElectronicDocuments: {},
          Customer: {},
          ExecutedBy: {},
          ContactPerson: {},
          CreatedBy: {},
        },
      }),
      p.Locale({
        sorting: [{ roleType: m.Locale.Name }],
      }),
      p.WorkEffortState({
        sorting: [{ roleType: m.WorkEffortState.Name }],
      }),
      p.Priority({
        predicate: {
          kind: 'Equals',
          propertyType: m.Priority.IsActive,
          value: true,
        },
        sorting: [{ roleType: m.Priority.Name }],
      }),
      p.WorkEffortPurpose({
        predicate: {
          kind: 'Equals',
          propertyType: this.m.WorkEffortPurpose.IsActive,
          value: true,
        },
        sorting: [{ roleType: m.WorkEffortPurpose.Name }],
      }),
      p.FacilityType({
        name: 'workshops',
        predicate: {
          kind: 'Equals',
          propertyType: m.FacilityType.UniqueId,
          value: '07d554f3-421b-47f3-915a-60b3639f7371',
        },
        select: {
          FacilitiesWhereFacilityType: {},
        },
        sorting: [{ roleType: m.Facility.Name }],
      }),
      p.WorkTask({
        objectId: this.editRequest.objectId,
        select: {
          WorkEffortFixedAssetAssignmentsWhereAssignment: {
            include: {
              FixedAsset: {
                SerialisedItem_PartWhereSerialisedItem: {
                  ProductCategoriesWhereProduct: {
                    PrimaryParent: {},
                  },
                },
              },
            },
          },
        },
      })
    );

    this.onPrePullInitialize(pulls);
  }

  onPostPull(pullResult: IPullResult) {
    this.object = pullResult.object('_object');

    this.onPostPullInitialize(pullResult);

    const internalOrganisation =
      this.fetcher.getInternalOrganisation(pullResult);
    this.employees = internalOrganisation.ActiveEmployees;

    this.workshops = pullResult.collection<Facility>('workshops');

    this.workEffortStates = pullResult.collection<WorkEffortState>(
      this.m.WorkEffortState
    );
    this.priorities = pullResult.collection<Priority>(this.m.Priority);
    this.workEffortPurposes = pullResult.collection<WorkEffortPurpose>(
      this.m.WorkEffortPurpose
    );
    this.workEffortFixedAssetAssignments =
      pullResult.collection<WorkEffortFixedAssetAssignment>(
        this.m.WorkTask.WorkEffortFixedAssetAssignmentsWhereAssignment
      );

    this.updateCustomer(this.object.Customer);
  }

  public contactPersonAdded(contact: Person): void {
    const organisationContactRelationship =
      this.allors.context.create<OrganisationContactRelationship>(
        this.m.OrganisationContactRelationship
      );
    organisationContactRelationship.Organisation = this.object
      .Customer as Organisation;
    organisationContactRelationship.Contact = contact;

    this.contacts.push(contact);
    this.object.ContactPerson = contact;
  }

  public contactMechanismAdded(
    partyContactMechanism: PartyContactMechanism
  ): void {
    this.contactMechanisms.push(partyContactMechanism.ContactMechanism);
    partyContactMechanism.Party = this.object.Customer;
    this.object.FullfillContactMechanism =
      partyContactMechanism.ContactMechanism;
  }

  public customerSelected(customer: any) {
    this.updateCustomer(customer);
  }

  private updateCustomer(party: Party) {
    const m = this.m;
    const { pullBuilder: pull } = m;
    const x = {};

    const pulls = [
      pull.Party({
        object: party,
        select: {
          CurrentPartyContactMechanisms: {
            include: {
              ContactMechanism: {
                PostalAddress_Country: x,
              },
            },
          },
        },
      }),
      pull.Party({
        object: party,
        select: {
          CurrentContacts: x,
        },
      }),
      pull.Party({
        object: party,
        select: {
          WorkEffortsWhereCustomer: {
            include: {
              WorkEffortState: x,
            },
          },
        },
      }),
      pull.Party({
        object: party,
        select: {
          CustomerRelationshipsWhereCustomer: {
            include: {
              InternalOrganisation: x,
              Agreements: {
                MaintenanceAgreement_WorkEffortType: {
                  UnifiedGood: x,
                  ProductCategory: x,
                },
              },
            },
          },
        },
      }),
    ];

    this.allors.context.pull(pulls).subscribe((pullResult) => {
      const partyContactMechanisms: PartyContactMechanism[] =
        pullResult.collection<PartyContactMechanism>(
          m.Party.CurrentPartyContactMechanisms
        );

      this.contactMechanisms = partyContactMechanisms?.map(
        (v: PartyContactMechanism) => v.ContactMechanism
      );

      this.contacts = pullResult.collection<Person>(m.Party.CurrentContacts);

      const allCustomerRelationships =
        pullResult.collection<CustomerRelationship>(
          m.Party.CustomerRelationshipsWhereCustomer
        );

      this.customerRelationship = allCustomerRelationships?.find((v) => {
        return v.InternalOrganisation.id === this.internalOrganisationId.value;
      });

      this.findAgreement();
    });
  }

  public setDescription(): void {
    if (this.object.Description == null) {
      this.object.Description =
        this.object.MaintenanceAgreement.WorkEffortType.Description;
    }
  }

  public findAgreement(): void {
    if (this.customerRelationship) {
      const activeAgreements = this.customerRelationship.Agreements?.filter(
        (v) => {
          const before = isBefore(
            new Date(v.FromDate),
            new Date(
              this.object.ScheduledStart ??
                this.object.ActualStart ??
                new Date()
            )
          );
          const sameDay = isSameDay(
            new Date(v.FromDate),
            new Date(
              this.object.ScheduledStart ??
                this.object.ActualStart ??
                new Date()
            )
          );
          const after =
            v.ThroughDate == null ||
            isAfter(
              new Date(v.ThroughDate),
              new Date(
                this.object.ScheduledStart ??
                  this.object.ActualStart ??
                  new Date()
              )
            );
          return (before || sameDay) && after;
        }
      );

      if (
        this.workEffortFixedAssetAssignments?.length === 1 &&
        activeAgreements?.length > 0
      ) {
        const serialisedItem = this.workEffortFixedAssetAssignments[0]
          .FixedAsset as SerialisedItem;

        const agreements1 = activeAgreements?.filter(
          (v: MaintenanceAgreement) => {
            return (
              v.WorkEffortType.UnifiedGood?.id ===
              serialisedItem.PartWhereSerialisedItem.id
            );
          }
        );

        const agreements2 = activeAgreements?.filter(
          (v: MaintenanceAgreement) => {
            return (
              v.WorkEffortType.ProductCategory?.id ===
              serialisedItem.PartWhereSerialisedItem
                ?.ProductCategoriesWhereProduct[0]?.id
            );
          }
        );

        const agreements3 = activeAgreements?.filter(
          (v: MaintenanceAgreement) => {
            return (
              v.WorkEffortType.ProductCategory?.id ===
              serialisedItem.PartWhereSerialisedItem
                ?.ProductCategoriesWhereProduct[0]?.PrimaryParent?.id
            );
          }
        );

        this.agreements = agreements1.concat(agreements2).concat(agreements3);
      } else {
        this.agreements = activeAgreements;
      }
    }
  }
}
