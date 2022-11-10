// <copyright file="PurchaseOrderApproverLevel1Tests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

using System.Linq;
using Allors.Database.Domain.TestPopulation;

namespace Allors.Database.Domain.Tests
{
    using Allors;
    using Xunit;

    public class PurchaseOrderApproverLevel1Tests : DomainTest, IClassFixture<Fixture>
    {
        public PurchaseOrderApproverLevel1Tests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void UserGroup()
        {
            var organisation = new OrganisationBuilder(this.Transaction).WithName("organisation").WithIsInternalOrganisation(true).Build();
            this.Transaction.Derive(true);

            Assert.True(organisation.ExistPurchaseOrderApproverLevel1UserGroup);

            organisation.RemovePurchaseOrderApproverLevel1UserGroup();
            this.Transaction.Derive();

            Assert.True(organisation.ExistPurchaseOrderApproverLevel1UserGroup);
        }

        [Fact]
        public void Grant()
        {
            var organisation = new OrganisationBuilder(this.Transaction).WithName("organisation").WithIsInternalOrganisation(true).Build();
            this.Transaction.Derive(true);

            Assert.True(organisation.ExistPurchaseOrderApproverLevel1Grant);
            Assert.Equal(new Roles(this.Transaction).PurchaseOrderApproverLevel1, organisation.PurchaseOrderApproverLevel1Grant.Role);
            Assert.Contains(organisation.PurchaseOrderApproverLevel1UserGroup, organisation.PurchaseOrderApproverLevel1Grant.SubjectGroups);

            organisation.RemovePurchaseOrderApproverLevel1Grant();

            this.Transaction.Derive(true);

            Assert.True(organisation.ExistPurchaseOrderApproverLevel1Grant);
            Assert.Equal(new Roles(this.Transaction).PurchaseOrderApproverLevel1, organisation.PurchaseOrderApproverLevel1Grant.Role);
            Assert.Contains(organisation.PurchaseOrderApproverLevel1UserGroup, organisation.PurchaseOrderApproverLevel1Grant.SubjectGroups);
        }

        [Fact]
        public void SecurityToken()
        {
            var organisation = new OrganisationBuilder(this.Transaction).WithName("organisation").WithIsInternalOrganisation(true).Build();
            this.Transaction.Derive(true);

            Assert.True(organisation.ExistPurchaseOrderApproverLevel1SecurityToken);
            Assert.Contains(organisation.PurchaseOrderApproverLevel1Grant, organisation.PurchaseOrderApproverLevel1SecurityToken.Grants);
        }

        [Fact]
        public void PurchaseOrderApprovers()
        {
            var organisation = new OrganisationBuilder(this.Transaction).WithName("organisation").WithIsInternalOrganisation(true).Build();
            this.Transaction.Derive(true);

            var approver = new PersonBuilder(this.Transaction)
                .WithUserName("approver")
                .WithFirstName("productquote")
                .WithLastName("approver")
                .Build();

            organisation.AddPurchaseOrderApproversLevel1(approver);

            this.Transaction.Derive(true);

            Assert.Contains(approver, organisation.PurchaseOrderApproverLevel1UserGroup.Members);
        }
    }

    public class PurchaseOrderApproverLevel1SecurityTests : DomainTest, IClassFixture<Fixture>
    {
        public PurchaseOrderApproverLevel1SecurityTests(Fixture fixture) : base(fixture) { }

        public override Config Config => new Config { SetupSecurity = true };

        [Fact]
        public void PurchaseOrder_Approve()
        {
            var organisation = new OrganisationBuilder(this.Transaction).WithName("organisation").WithIsInternalOrganisation(true).Build();
            this.Transaction.Derive();
            this.Transaction.Commit();

            var approver = new PersonBuilder(this.Transaction)
                .WithUserName("approver")
                .WithFirstName("purchaseorder")
                .WithLastName("approver")
                .Build();

            organisation.AddPurchaseOrderApproversLevel1(approver);

            this.Transaction.Derive();

            var supplier = new OrganisationBuilder(this.Transaction).WithName("supplier").Build();
            new SupplierRelationshipBuilder(this.Transaction)
                .WithInternalOrganisation(organisation)
                .WithSupplier(supplier)
                .WithNeedsApproval(true)
                .WithApprovalThresholdLevel1(10)
                .Build();

            var mechelen = new CityBuilder(this.Transaction).WithName("Mechelen").Build();
            ContactMechanism takenViaContactMechanism = new PostalAddressBuilder(this.Transaction).WithPostalAddressBoundary(mechelen).WithAddress1("Haverwerf 15").Build();
            var supplierContactMechanism = new PartyContactMechanismBuilder(this.Transaction)
                .WithParty(supplier)
                .WithContactMechanism(takenViaContactMechanism)
                .WithUseAsDefault(true)
                .WithContactPurpose(new ContactMechanismPurposes(this.Transaction).BillingAddress)
                .Build();

            this.Transaction.Derive();

            var order = new PurchaseOrderBuilder(this.Transaction)
                .WithOrderedBy(organisation)
                .WithTakenViaSupplier(supplier)
                .WithAssignedBillToContactMechanism(takenViaContactMechanism)
                .WithStoredInFacility(new Facilities(this.Transaction).FindBy(M.Facility.FacilityType, new FacilityTypes(this.Transaction).Warehouse))
                .Build();

            var finishedGood = new NonUnifiedPartBuilder(this.Transaction)
                .WithNonSerialisedDefaults(organisation)
                .Build();

            var item = new PurchaseOrderItemBuilder(this.Transaction)
                .WithPart(finishedGood)
                .WithQuantityOrdered(3)
                .WithAssignedUnitPrice(5)
                .Build();

            order.AddPurchaseOrderItem(item);

            this.Transaction.Derive();

            order.SetReadyForProcessing();

            this.Transaction.Derive();
            this.Transaction.Commit();

            Assert.True(order.PurchaseOrderState.IsAwaitingApprovalLevel1);

            this.Transaction.SetUser(approver);

            var acl = new DatabaseAccessControl(this.Security, approver)[order];
            Assert.True(acl.CanExecute(M.PurchaseOrder.Approve));
        }

        [Fact]
        public void PurchaseOrderApproval()
        {
            var organisation = new OrganisationBuilder(this.Transaction).WithName("organisation").WithIsInternalOrganisation(true).Build();
            this.Transaction.Derive();

            var approver = new PersonBuilder(this.Transaction)
                .WithUserName("approver")
                .WithFirstName("purchaseorder")
                .WithLastName("approver")
                .Build();

            organisation.AddPurchaseOrderApproversLevel1(approver);

            this.Transaction.Derive();

            var supplier = new OrganisationBuilder(this.Transaction).WithName("supplier").Build();
            new SupplierRelationshipBuilder(this.Transaction)
                .WithInternalOrganisation(organisation)
                .WithSupplier(supplier)
                .WithNeedsApproval(true)
                .WithApprovalThresholdLevel1(10)
                .Build();

            var mechelen = new CityBuilder(this.Transaction).WithName("Mechelen").Build();
            ContactMechanism takenViaContactMechanism = new PostalAddressBuilder(this.Transaction).WithPostalAddressBoundary(mechelen).WithAddress1("Haverwerf 15").Build();
            var supplierContactMechanism = new PartyContactMechanismBuilder(this.Transaction)
                .WithParty(supplier)
                .WithContactMechanism(takenViaContactMechanism)
                .WithUseAsDefault(true)
                .WithContactPurpose(new ContactMechanismPurposes(this.Transaction).BillingAddress)
                .Build();

            this.Transaction.Derive();

            var order = new PurchaseOrderBuilder(this.Transaction)
                .WithOrderedBy(organisation)
                .WithTakenViaSupplier(supplier)
                .WithAssignedBillToContactMechanism(takenViaContactMechanism)
                .WithStoredInFacility(new Facilities(this.Transaction).FindBy(M.Facility.FacilityType, new FacilityTypes(this.Transaction).Warehouse))
                .Build();

            var finishedGood = new NonUnifiedPartBuilder(this.Transaction)
                .WithNonSerialisedDefaults(organisation)
                .Build();

            var item = new PurchaseOrderItemBuilder(this.Transaction)
                .WithPart(finishedGood)
                .WithQuantityOrdered(3)
                .WithAssignedUnitPrice(5)
                .Build();

            order.AddPurchaseOrderItem(item);

            this.Transaction.Derive();

            order.SetReadyForProcessing();

            this.Transaction.Derive();

            this.Transaction.SetUser(approver);

            var purchaseOrderApproval = order.PurchaseOrderApprovalsLevel1WherePurchaseOrder.First();

            var acl = new DatabaseAccessControl(this.Security, approver)[purchaseOrderApproval];
            Assert.True(acl.CanWrite(M.ApproveTask.Comment));
            Assert.True(acl.CanExecute(M.ApproveTask.Approve));
        }
    }
}
