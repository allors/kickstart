import { Component, Self } from '@angular/core';
import { NgForm } from '@angular/forms';

import { Pull, IPullResult } from '@allors/system/workspace/domain';
import {
  Currency,
  EmailAddress,
  EmailFrequency,
  Enumeration,
  GenderType,
  InternalOrganisation,
  Locale,
  Organisation,
  OrganisationContactKind,
  OrganisationContactRelationship,
  PartyContactMechanism,
  Person,
  Salutation,
  User,
  UserGroup,
} from '@allors/default/workspace/domain';
import { M } from '@allors/default/workspace/meta';
import {
  ErrorService,
  AllorsFormComponent,
  SearchFactory,
  SingletonId,
  UserId,
} from '@allors/base/workspace/angular/foundation';
import { ContextService } from '@allors/base/workspace/angular/foundation';
import { FetcherService } from '@allors/apps-intranet/workspace/angular-material';

@Component({
  selector: 'person-form',
  templateUrl: './person-form.component.html',
  providers: [ContextService],
})
export class PersonFormComponent extends AllorsFormComponent<Person> {
  readonly m: M;
  emailAddresses: string[] = [];
  isAdministrator: boolean;

  internalOrganisation: InternalOrganisation;
  organisation: Organisation;
  aviacos: InternalOrganisation[];
  organisationsFilter: SearchFactory;

  organisationContactKinds: OrganisationContactKind[];
  selectedContactKinds: OrganisationContactKind[] = [];

  locales: Locale[];
  genders: Enumeration[];
  salutations: Enumeration[];
  public confirmPassword: string;

  currencies: Currency[];
  emailFrequencies: EmailFrequency[];
  canWriteMembers: boolean;

  userGroups: UserGroup[];
  selectedUserGroups: UserGroup[] = [];
  creatorUserGroup: UserGroup;
  user: Person;

  constructor(
    @Self() public allors: ContextService,
    errorService: ErrorService,
    form: NgForm,
    private singletonId: SingletonId,
    private fetcher: FetcherService,
    private userId: UserId
  ) {
    super(allors, errorService, form);
    this.m = allors.metaPopulation as M;

    this.organisationsFilter = new SearchFactory({
      objectType: this.m.Organisation,
      roleTypes: [this.m.Organisation.Name],
    });
  }

  onPrePull(pulls: Pull[]): void {
    const { m } = this;
    const { pullBuilder: p } = m;

    pulls.push(
      this.fetcher.internalOrganisation,
      p.InternalOrganisation({
        predicate: {
          kind: 'Equals',
          propertyType: this.m.Organisation.IsInternalOrganisation,
          value: true,
        },
        include: {
          ProductQuoteApprovers: {},
          PurchaseOrderApproversLevel1: {},
          PurchaseOrderApproversLevel2: {},
          PurchaseInvoiceApprovers: {},
          BlueCollarWorkers: {},
          StockManagers: {},
          LocalSalesAccountManagers: {},
          LocalAdministrators: {},
          LocalEmployees: {},
        },
      }),
      p.Singleton({
        objectId: this.singletonId.value,
        select: {
          Locales: {
            include: {
              Language: {},
              Country: {},
            },
          },
        },
      }),
      p.Person({
        objectId: this.userId.value,
        include: { UserGroupsWhereMember: {} },
      }),
      p.EmailFrequency({
        predicate: {
          kind: 'Equals',
          propertyType: m.EmailFrequency.IsActive,
          value: true,
        },
        sorting: [{ roleType: m.EmailFrequency.Name }],
      }),
      p.Currency({
        predicate: {
          kind: 'Equals',
          propertyType: m.Currency.IsActive,
          value: true,
        },
        sorting: [{ roleType: m.Currency.Name }],
      }),
      p.GenderType({
        predicate: {
          kind: 'Equals',
          propertyType: m.GenderType.IsActive,
          value: true,
        },
        sorting: [{ roleType: m.GenderType.Name }],
      }),
      p.Salutation({
        predicate: {
          kind: 'Equals',
          propertyType: m.Salutation.IsActive,
          value: true,
        },
        sorting: [{ roleType: m.Salutation.Name }],
      }),
      p.UserGroup({
        predicate: {
          kind: 'Equals',
          propertyType: m.UserGroup.IsSelectable,
          value: true,
        },
        include: {
          InMembers: {},
          OutMembers: {},
        },
        sorting: [{ roleType: m.Role.Name }],
      }),
      p.UserGroup({
        name: 'CreatorUserGroup',
        predicate: {
          kind: 'Equals',
          propertyType: m.UserGroup.UniqueId,
          value: 'f0d8132b-79d6-4a30-a866-ef6e5c952761',
        },
        include: {
          InMembers: {},
          OutMembers: {},
        },
      }),
      p.OrganisationContactKind({
        sorting: [{ roleType: m.OrganisationContactKind.Description }],
      })
    );

    if (this.editRequest) {
      pulls.push(
        p.Person({
          name: '_object',
          objectId: this.editRequest.objectId,
          include: {
            PreferredCurrency: {},
            Gender: {},
            Salutation: {},
            Locale: {},
            Picture: {},
          },
        }),
        p.Person({
          objectId: this.editRequest.objectId,
          select: {
            PartyContactMechanismsWhereParty: {
              include: {
                ContactMechanism: {
                  ContactMechanismType: {},
                },
              },
            },
          },
        }),
        p.Person({
          name: 'ActiveUserGroups',
          objectId: this.editRequest.objectId,
          select: {
            UserGroupsWhereMember: {},
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

    this.onPostPullInitialize(pullResult);

    this.internalOrganisation =
      this.fetcher.getInternalOrganisation(pullResult);
    this.aviacos = pullResult.collection<InternalOrganisation>(
      this.m.InternalOrganisation
    );
    this.currencies = pullResult.collection<Currency>(this.m.Currency);
    this.locales =
      pullResult.collection<Locale>(this.m.Singleton.Locales) || [];
    this.genders = pullResult.collection<GenderType>(this.m.GenderType);
    this.salutations = pullResult.collection<Salutation>(this.m.Salutation);
    this.emailFrequencies = pullResult.collection<EmailFrequency>(
      this.m.EmailFrequency
    );
    this.organisationContactKinds =
      pullResult.collection<OrganisationContactKind>(
        this.m.OrganisationContactKind
      );

    this.userGroups = pullResult.collection<UserGroup>(this.m.UserGroup);
    this.creatorUserGroup =
      pullResult.collection<UserGroup>('CreatorUserGroup')[0];
    this.canWriteMembers = this.userGroups[0].canWriteMembers;

    const partyContactMechanisms: PartyContactMechanism[] =
      pullResult.collection<PartyContactMechanism>(
        this.m.Party.PartyContactMechanismsWhereParty
      );
    this.emailAddresses =
      partyContactMechanisms
        ?.filter((v) => v.ContactMechanism.strategy.cls === this.m.EmailAddress)
        ?.map((v) => v.ContactMechanism)
        .map((v: EmailAddress) => v.ElectronicAddressString) ?? [];

    this.user = pullResult.object<Person>(this.m.Person);
    this.isAdministrator =
      this.user.UserGroupsWhereMember.findIndex(
        (v) => v.UniqueId === 'cdc04209-683b-429c-bed2-440851f430df'
      ) > -1;

    if (this.createRequest) {
      this.object.CollectiveWorkEffortInvoice = false;
      this.object.PreferredCurrency =
        this.internalOrganisation.PreferredCurrency;
    }

    if (this.editRequest) {
      this.selectedUserGroups =
        pullResult.collection<UserGroup>('ActiveUserGroups');
    }
  }

  public onUserGroupChange(event): void {
    const userGroup = event.source.value as UserGroup;
    const user = this.object as User;

    if (event.isUserInput) {
      if (event.source.selected) {
        if (userGroup.InternalOrganisationWhereBlueCollarWorkerUserGroup) {
          userGroup.InternalOrganisationWhereBlueCollarWorkerUserGroup.addBlueCollarWorker(
            this.object
          );
        } else if (
          userGroup.InternalOrganisationWhereLocalAdministratorUserGroup
        ) {
          userGroup.InternalOrganisationWhereLocalAdministratorUserGroup.addLocalAdministrator(
            this.object
          );
        } else if (
          userGroup.InternalOrganisationWhereProductQuoteApproverUserGroup
        ) {
          userGroup.InternalOrganisationWhereProductQuoteApproverUserGroup.addProductQuoteApprover(
            this.object
          );
        } else if (
          userGroup.InternalOrganisationWherePurchaseInvoiceApproverUserGroup
        ) {
          userGroup.InternalOrganisationWherePurchaseInvoiceApproverUserGroup.addPurchaseInvoiceApprover(
            this.object
          );
        } else if (
          userGroup.InternalOrganisationWherePurchaseOrderApproverLevel1UserGroup
        ) {
          userGroup.InternalOrganisationWherePurchaseOrderApproverLevel1UserGroup.addPurchaseOrderApproversLevel1(
            this.object
          );
        } else if (
          userGroup.InternalOrganisationWherePurchaseOrderApproverLevel2UserGroup
        ) {
          userGroup.InternalOrganisationWherePurchaseOrderApproverLevel2UserGroup.addPurchaseOrderApproversLevel2(
            this.object
          );
        } else if (userGroup.InternalOrganisationWhereStockManagerUserGroup) {
          userGroup.InternalOrganisationWhereStockManagerUserGroup.addStockManager(
            this.object
          );
        } else if (userGroup.InternalOrganisationWhereLocalEmployeeUserGroup) {
          userGroup.InternalOrganisationWhereLocalEmployeeUserGroup.addLocalEmployee(
            this.object
          );
        } else if (
          userGroup.InternalOrganisationWhereLocalSalesAccountManagerUserGroup
        ) {
          userGroup.InternalOrganisationWhereLocalSalesAccountManagerUserGroup.addLocalSalesAccountManager(
            this.object
          );
        } else {
          userGroup.addInMember(user);
        }
      } else {
        if (userGroup.InternalOrganisationWhereBlueCollarWorkerUserGroup) {
          userGroup.InternalOrganisationWhereBlueCollarWorkerUserGroup.removeBlueCollarWorker(
            this.object
          );
        } else if (
          userGroup.InternalOrganisationWhereLocalAdministratorUserGroup
        ) {
          userGroup.InternalOrganisationWhereLocalAdministratorUserGroup.removeLocalAdministrator(
            this.object
          );
        } else if (
          userGroup.InternalOrganisationWhereProductQuoteApproverUserGroup
        ) {
          userGroup.InternalOrganisationWhereProductQuoteApproverUserGroup.removeProductQuoteApprover(
            this.object
          );
        } else if (
          userGroup.InternalOrganisationWherePurchaseInvoiceApproverUserGroup
        ) {
          userGroup.InternalOrganisationWherePurchaseInvoiceApproverUserGroup.removePurchaseInvoiceApprover(
            this.object
          );
        } else if (
          userGroup.InternalOrganisationWherePurchaseOrderApproverLevel1UserGroup
        ) {
          userGroup.InternalOrganisationWherePurchaseOrderApproverLevel1UserGroup.removePurchaseOrderApproversLevel1(
            this.object
          );
        } else if (
          userGroup.InternalOrganisationWherePurchaseOrderApproverLevel2UserGroup
        ) {
          userGroup.InternalOrganisationWherePurchaseOrderApproverLevel2UserGroup.removePurchaseOrderApproversLevel2(
            this.object
          );
        } else if (userGroup.InternalOrganisationWhereStockManagerUserGroup) {
          userGroup.InternalOrganisationWhereStockManagerUserGroup.removeStockManager(
            this.object
          );
        } else if (userGroup.InternalOrganisationWhereLocalEmployeeUserGroup) {
          userGroup.InternalOrganisationWhereLocalEmployeeUserGroup.removeLocalEmployee(
            this.object
          );
        } else if (
          userGroup.InternalOrganisationWhereLocalSalesAccountManagerUserGroup
        ) {
          userGroup.InternalOrganisationWhereLocalSalesAccountManagerUserGroup.removeLocalSalesAccountManager(
            this.object
          );
        } else {
          userGroup.addOutMember(user);
        }
      }
    }
  }

  public override save(): void {
    this.onSave();
    super.save();
  }

  private onSave(): void {
    if (
      this.object.UserEmail != null &&
      this.emailAddresses.indexOf(this.object.UserEmail) == -1
    ) {
      const emailAddress = this.allors.context.create<EmailAddress>(
        this.m.EmailAddress
      );
      emailAddress.ElectronicAddressString = this.object.UserEmail;

      const partyContactMechanism =
        this.allors.context.create<PartyContactMechanism>(
          this.m.PartyContactMechanism
        );
      partyContactMechanism.ContactMechanism = emailAddress;

      partyContactMechanism.Party = this.object;
      this.emailAddresses.push(this.object.UserEmail);
    }

    if (this.organisation != null) {
      const organisationContactRelationship =
        this.allors.context.create<OrganisationContactRelationship>(
          this.m.OrganisationContactRelationship
        );
      organisationContactRelationship.Contact = this.object;
      organisationContactRelationship.Organisation = this.organisation;
      organisationContactRelationship.ContactKinds = this.selectedContactKinds;
    }

    if (this.selectedUserGroups.length == 0) {
      this.creatorUserGroup.addOutMember(this.object);
    } else {
      this.creatorUserGroup.addInMember(this.object);
    }
  }
}
