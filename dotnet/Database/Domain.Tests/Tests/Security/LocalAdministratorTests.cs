// <copyright file="LocalAdministratorTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

using Allors.Database.Domain.TestPopulation;

namespace Allors.Database.Domain.Tests
{
    using System.Linq;
    using Xunit;

    public class LocalAdministratorTests : DomainTest, IClassFixture<Fixture>
    {
        public LocalAdministratorTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void UserGroup()
        {
            var organisation = new OrganisationBuilder(this.Transaction).WithName("organisation").WithIsInternalOrganisation(true).Build();
            this.Transaction.Derive();

            Assert.True(organisation.ExistLocalAdministratorUserGroup);
            organisation.RemoveLocalAdministratorUserGroup();
            this.Transaction.Derive();

            Assert.True(organisation.ExistLocalAdministratorUserGroup);
        }

        [Fact]
        public void Grant()
        {
            var organisation = new OrganisationBuilder(this.Transaction).WithName("organisation").WithIsInternalOrganisation(true).Build();
            this.Transaction.Derive();

            Assert.True(organisation.ExistLocalAdministratorGrant);
            Assert.Equal(new Roles(this.Transaction).LocalAdministrator, organisation.LocalAdministratorGrant.Role);
            Assert.Contains(organisation.LocalAdministratorUserGroup, organisation.LocalAdministratorGrant.SubjectGroups);

            organisation.RemoveLocalAdministratorGrant();
            this.Transaction.Derive();

            Assert.True(organisation.ExistLocalAdministratorGrant);
            Assert.Equal(new Roles(this.Transaction).LocalAdministrator, organisation.LocalAdministratorGrant.Role);
            Assert.Contains(organisation.LocalAdministratorUserGroup, organisation.LocalAdministratorGrant.SubjectGroups);
        }

        [Fact]
        public void SecurityToken()
        {
            var organisation = new OrganisationBuilder(this.Transaction).WithName("organisation").WithIsInternalOrganisation(true).Build();
            this.Transaction.Derive();

            Assert.True(organisation.ExistLocalAdministratorSecurityToken);
            Assert.Contains(organisation.LocalAdministratorGrant, organisation.LocalAdministratorSecurityToken.Grants);
        }

        [Fact]
        public void LocalAdministrators()
        {
            var organisation = new OrganisationBuilder(this.Transaction).WithName("organisation").WithIsInternalOrganisation(true).Build();
            this.Transaction.Derive();

            var localAdmin = new PersonBuilder(this.Transaction)
                .WithUserName("localAdmin")
                .WithFirstName("blue-collar")
                .WithLastName("localAdmin")
                .Build();

            organisation.AddLocalAdministrator(localAdmin);

            this.Transaction.Derive();

            Assert.Contains(localAdmin, organisation.LocalAdministratorUserGroup.Members);
        }
    }

    public class LocalAdministratorSecurityTests : DomainTest, IClassFixture<Fixture>
    {
        public LocalAdministratorSecurityTests(Fixture fixture) : base(fixture) { }

        public override Config Config => new Config { SetupSecurity = true };

        [Fact]
        public void WorkTaskOwnInternalOrganisation()
        {
            var localAdmin = new PersonBuilder(this.Transaction)
                .WithUserName("localadmin")
                .WithLastName("localAdmin")
                .Build();

            this.InternalOrganisation.AddLocalAdministrator(localAdmin);
            this.Transaction.Derive();

            this.Transaction.SetUser(localAdmin);

            var workTask = new WorkTaskBuilder(this.Transaction).WithName("Activity").WithCustomer(this.Customer).WithTakenBy(this.InternalOrganisation).Build();

            this.Transaction.Derive();
            this.Transaction.Commit();

            Assert.True(workTask.ExistSecurityTokens);

            var acl = new DatabaseAccessControl(this.Security, localAdmin)[workTask];
            Assert.True(acl.CanRead(M.WorkTask.WorkDone));
            Assert.True(acl.CanWrite(M.WorkTask.WorkDone));
        }

        [Fact]
        public void WorkTaskOtherInternalOrganisation()
        {
            var localAdmin = new PersonBuilder(this.Transaction)
                .WithUserName("localadmin")
                .WithLastName("localAdmin")
                .Build();

            this.InternalOrganisation.AddLocalAdministrator(localAdmin);
            this.Transaction.Derive();

            var otherInternalOrganisation = new OrganisationBuilder(this.Transaction).WithIsInternalOrganisation(true).WithName("other internalOrganisation").Build();
            this.Transaction.Derive();

            this.Transaction.SetUser(localAdmin);

            var workTask = new WorkTaskBuilder(this.Transaction).WithName("Activity").WithCustomer(this.Customer).WithTakenBy(otherInternalOrganisation).Build();

            this.Transaction.Derive();
            this.Transaction.Commit();

            Assert.True(workTask.ExistSecurityTokens);

            var acl = new DatabaseAccessControl(this.Security, localAdmin)[workTask];
            Assert.False(acl.CanRead(M.WorkTask.WorkDone));
            Assert.False(acl.CanWrite(M.WorkTask.WorkDone));
        }

        [Fact]
        public void WorkEffortInventoryAssignmentOwnInternalOrganisation()
        {
            var localAdmin = new PersonBuilder(this.Transaction)
                .WithUserName("localadmin")
                .WithLastName("localAdmin")
                .Build();

            this.InternalOrganisation.AddLocalAdministrator(localAdmin);
            this.Transaction.Derive();

            this.Transaction.SetUser(localAdmin);

            var workTask = new WorkTaskBuilder(this.Transaction).WithName("Activity").WithCustomer(this.Customer).WithTakenBy(this.InternalOrganisation).Build();

            this.Transaction.Derive();

            var part = new NonUnifiedPartBuilder(this.Transaction)
                .WithProductIdentification(new PartNumberBuilder(this.Transaction)
                    .WithIdentification("P1")
                    .WithProductIdentificationType(new ProductIdentificationTypes(this.Transaction).Part).Build())
                .Build();

            this.Transaction.Derive();
            this.Transaction.Commit();

            new InventoryItemTransactionBuilder(this.Transaction).WithQuantity(100).WithReason(new InventoryTransactionReasons(this.Transaction).Unknown).WithPart(part).Build();
            this.Transaction.Derive();

            var inventoryAssignment = new WorkEffortInventoryAssignmentBuilder(this.Transaction)
                .WithAssignment(workTask)
                .WithInventoryItem(part.InventoryItemsWherePart.First())
                .WithQuantity(10)
                .Build();

            this.Transaction.Derive();

            Assert.Equal(new WorkEffortStates(this.Transaction).Created, workTask.WorkEffortState);

            Assert.True(inventoryAssignment.ExistSecurityTokens);

            var acl = new DatabaseAccessControl(this.Security, localAdmin)[inventoryAssignment];
            Assert.True(acl.CanRead(M.WorkEffortInventoryAssignment.InventoryItem));
            Assert.True(acl.CanWrite(M.WorkEffortInventoryAssignment.InventoryItem));
            Assert.True(acl.CanRead(M.WorkEffortInventoryAssignment.Quantity));
            Assert.True(acl.CanWrite(M.WorkEffortInventoryAssignment.Quantity));
            Assert.True(acl.CanRead(M.WorkEffortInventoryAssignment.AssignedBillableQuantity));
            Assert.True(acl.CanWrite(M.WorkEffortInventoryAssignment.AssignedBillableQuantity));
            Assert.True(acl.CanRead(M.WorkEffortInventoryAssignment.UnitSellingPrice));
            Assert.True(acl.CanRead(M.WorkEffortInventoryAssignment.AssignedUnitSellingPrice));
            Assert.True(acl.CanWrite(M.WorkEffortInventoryAssignment.AssignedUnitSellingPrice));
            Assert.True(acl.CanRead(M.WorkEffortInventoryAssignment.UnitPurchasePrice));
            Assert.True(acl.CanRead(M.WorkEffortInventoryAssignment.CostOfGoodsSold));
        }

        [Fact]
        public void WorkEffortInventoryAssignmentOtherInternalOrganisation()
        {
            var localAdmin = new PersonBuilder(this.Transaction)
                .WithUserName("localadmin")
                .WithLastName("localAdmin")
                .Build();

            this.InternalOrganisation.AddLocalAdministrator(localAdmin);
            this.Transaction.Derive();

            var otherInternalOrganisation = new OrganisationBuilder(this.Transaction).WithIsInternalOrganisation(true).WithName("other internalOrganisation").Build();
            this.Transaction.Derive();

            var workTask = new WorkTaskBuilder(this.Transaction).WithName("Activity").WithCustomer(this.Customer).WithTakenBy(otherInternalOrganisation).Build();

            this.Transaction.Derive();

            var part = new NonUnifiedPartBuilder(this.Transaction)
                .WithProductIdentification(new PartNumberBuilder(this.Transaction)
                    .WithIdentification("P1")
                    .WithProductIdentificationType(new ProductIdentificationTypes(this.Transaction).Part).Build())
                .Build();

            this.Transaction.Derive();

            new InventoryItemTransactionBuilder(this.Transaction).WithQuantity(100).WithReason(new InventoryTransactionReasons(this.Transaction).Unknown).WithPart(part).Build();
            this.Transaction.Derive();

            this.Transaction.SetUser(localAdmin);

            var inventoryAssignment = new WorkEffortInventoryAssignmentBuilder(this.Transaction)
                .WithAssignment(workTask)
                .WithInventoryItem(part.InventoryItemsWherePart.First())
                .WithQuantity(10)
                .Build();

            this.Transaction.Derive();
            this.Transaction.Commit();

            Assert.Equal(new WorkEffortStates(this.Transaction).Created, workTask.WorkEffortState);

            Assert.True(inventoryAssignment.ExistSecurityTokens);

            var acl = new DatabaseAccessControl(this.Security, localAdmin)[inventoryAssignment];
            Assert.False(acl.CanRead(M.WorkEffortInventoryAssignment.InventoryItem));
            Assert.False(acl.CanWrite(M.WorkEffortInventoryAssignment.InventoryItem));
            Assert.False(acl.CanRead(M.WorkEffortInventoryAssignment.Quantity));
            Assert.False(acl.CanWrite(M.WorkEffortInventoryAssignment.Quantity));
            Assert.False(acl.CanRead(M.WorkEffortInventoryAssignment.AssignedBillableQuantity));
            Assert.False(acl.CanWrite(M.WorkEffortInventoryAssignment.AssignedBillableQuantity));
            Assert.False(acl.CanRead(M.WorkEffortInventoryAssignment.UnitSellingPrice));
            Assert.False(acl.CanWrite(M.WorkEffortInventoryAssignment.UnitSellingPrice));
            Assert.False(acl.CanRead(M.WorkEffortInventoryAssignment.AssignedUnitSellingPrice));
            Assert.False(acl.CanWrite(M.WorkEffortInventoryAssignment.AssignedUnitSellingPrice));
            Assert.False(acl.CanRead(M.WorkEffortInventoryAssignment.UnitPurchasePrice));
            Assert.False(acl.CanWrite(M.WorkEffortInventoryAssignment.UnitPurchasePrice));
            Assert.False(acl.CanRead(M.WorkEffortInventoryAssignment.CostOfGoodsSold));
            Assert.False(acl.CanWrite(M.WorkEffortInventoryAssignment.CostOfGoodsSold));
        }

        [Fact]
        public void WorkEffortFixedAssetAssignmentOwnInternalOrganisation()
        {
            var localAdmin = new PersonBuilder(this.Transaction)
                .WithUserName("localadmin")
                .WithLastName("localAdmin")
                .Build();

            this.InternalOrganisation.AddLocalAdministrator(localAdmin);

            var workTask = new WorkTaskBuilder(this.Transaction).WithName("Activity").WithCustomer(this.Customer).WithTakenBy(this.InternalOrganisation).Build();
            var fixedAsset = new SerialisedItemBuilder(this.Transaction).Build();

            this.Transaction.Derive();

            this.Transaction.SetUser(localAdmin);

            var assignment = new WorkEffortFixedAssetAssignmentBuilder(this.Transaction)
                .WithAssignment(workTask)
                .WithFixedAsset(fixedAsset)
                .Build();

            this.Transaction.Derive();

            Assert.True(workTask.ExistSecurityTokens);

            var acl = new DatabaseAccessControl(this.Security, localAdmin)[assignment];
            Assert.True(acl.CanRead(M.WorkEffortFixedAssetAssignment.AllocatedCost));
            Assert.True(acl.CanWrite(M.WorkEffortFixedAssetAssignment.AllocatedCost));
            Assert.True(acl.CanExecute(M.WorkEffortFixedAssetAssignment.Delete));
        }

        [Fact]
        public void WorkEffortFixedAssetAssignmentOtherInternalOrganisation()
        {
            var localAdmin = new PersonBuilder(this.Transaction)
                .WithUserName("localadmin")
                .WithLastName("localAdmin")
                .Build();

            this.InternalOrganisation.AddLocalAdministrator(localAdmin);

            var userGroups = new UserGroups(this.Transaction);
            userGroups.Creators.AddMember(localAdmin);

            var otherInternalOrganisation = new OrganisationBuilder(this.Transaction).WithIsInternalOrganisation(true).WithName("other internalOrganisation").WithWorkEffortSequence(new WorkEffortSequences(this.Transaction).RestartOnFiscalYear).Build();
            var workTask = new WorkTaskBuilder(this.Transaction).WithName("Activity").WithCustomer(this.Customer).WithTakenBy(otherInternalOrganisation).Build();
            var fixedAsset = new SerialisedItemBuilder(this.Transaction).Build();

            this.Transaction.Derive();

            this.Transaction.SetUser(localAdmin);

            var assignment = new WorkEffortFixedAssetAssignmentBuilder(this.Transaction)
                .WithAssignment(workTask)
                .WithFixedAsset(fixedAsset)
                .Build();

            this.Transaction.Derive();

            Assert.True(workTask.ExistSecurityTokens);

            var acl = new DatabaseAccessControl(this.Security, localAdmin)[assignment];
            Assert.False(acl.CanRead(M.WorkEffortFixedAssetAssignment.AllocatedCost));
            Assert.False(acl.CanWrite(M.WorkEffortFixedAssetAssignment.AllocatedCost));
            Assert.False(acl.CanExecute(M.WorkEffortFixedAssetAssignment.Delete));
        }

        [Fact]
        public void TimeEntryOwnInternalOrganisation()
        {
            var localAdmin = new PersonBuilder(this.Transaction)
                .WithUserName("localadmin")
                .WithLastName("localAdmin")
                .Build();

            this.InternalOrganisation.AddLocalAdministrator(localAdmin);
            new EmploymentBuilder(this.Transaction).WithEmployee(localAdmin).WithEmployer(this.InternalOrganisation).Build();

            this.Transaction.Derive();

            this.Transaction.SetUser(localAdmin);

            var workTask = new WorkTaskBuilder(this.Transaction).WithName("Activity").WithCustomer(this.Customer).WithTakenBy(this.InternalOrganisation).Build();

            var timeEntry = new TimeEntryBuilder(this.Transaction)
                .WithRateType(new RateTypes(this.Transaction).StandardRate)
                .WithFromDate(this.Transaction.Now())
                .WithWorkEffort(workTask)
                .Build();

            localAdmin.TimeSheetWhereWorker.AddTimeEntry(timeEntry);

            this.Transaction.Derive();
            this.Transaction.Commit();

            Assert.True(workTask.ExistSecurityTokens);

            var acl = new DatabaseAccessControl(this.Security, localAdmin)[timeEntry];
            Assert.True(acl.CanRead(M.TimeEntry.ThroughDate));
            Assert.True(acl.CanWrite(M.TimeEntry.ThroughDate));
        }

        [Fact(Skip = "TODO: Martien")]
        public void TimeEntryOtherInternalOrganisation()
        {
            var localAdmin = new PersonBuilder(this.Transaction)
                .WithUserName("localadmin")
                .WithLastName("localAdmin")
                .Build();

            this.InternalOrganisation.AddLocalAdministrator(localAdmin);

            var userGroups = new UserGroups(this.Transaction);
            userGroups.Creators.AddMember(localAdmin);

            this.Transaction.Derive();

            var otherInternalOrganisation = new OrganisationBuilder(this.Transaction).WithIsInternalOrganisation(true).WithName("other internalOrganisation").Build();
            this.Transaction.Derive();

            var worker = new PersonBuilder(this.Transaction)
                .WithUserName("worker")
                .WithFirstName("blue-collar")
                .WithLastName("worker")
                .Build();

            otherInternalOrganisation.AddBlueCollarWorker(worker);
            new EmploymentBuilder(this.Transaction).WithEmployee(worker).WithEmployer(otherInternalOrganisation).Build();

            userGroups.Creators.AddMember(worker);

            this.Transaction.Derive();

            var workTask = new WorkTaskBuilder(this.Transaction).WithName("Activity").WithCustomer(this.Customer).WithTakenBy(otherInternalOrganisation).Build();

            this.Transaction.SetUser(worker);

            var timeEntry = new TimeEntryBuilder(this.Transaction)
                .WithRateType(new RateTypes(this.Transaction).StandardRate)
                .WithFromDate(this.Transaction.Now())
                .WithWorkEffort(workTask)
                .Build();

            worker.TimeSheetWhereWorker.AddTimeEntry(timeEntry);

            this.Transaction.Derive();
            this.Transaction.Commit();

            Assert.True(workTask.ExistSecurityTokens);

            var acl = new DatabaseAccessControl(this.Security, localAdmin)[timeEntry];
            Assert.False(acl.CanRead(M.TimeEntry.ThroughDate));
            Assert.False(acl.CanWrite(M.TimeEntry.ThroughDate));
        }

        [Fact]
        public void PurchaseOrderOwnInternalOrganisation()
        {
            var localAdmin = new PersonBuilder(this.Transaction)
                .WithUserName("localadmin")
                .WithLastName("localAdmin")
                .Build();

            this.InternalOrganisation.AddLocalAdministrator(localAdmin);
            this.Transaction.Derive();

            this.Transaction.SetUser(localAdmin);

            var purchaseOrder = new PurchaseOrderBuilder(this.Transaction).WithOrderedBy(this.InternalOrganisation).WithTakenViaSupplier(this.Supplier).Build();

            this.Transaction.Derive();

            var acl = new DatabaseAccessControl(this.Security, localAdmin)[purchaseOrder];
            Assert.True(acl.CanRead(M.PurchaseOrder.Description));
            Assert.True(acl.CanWrite(M.PurchaseOrder.Description));

            var purchaseOrderItem = new PurchaseOrderItemBuilder(this.Transaction).WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).Other).WithQuantityOrdered(1).WithAssignedUnitPrice(1).Build();
            purchaseOrder.AddPurchaseOrderItem(purchaseOrderItem);

            this.Transaction.Derive();

            Assert.True(purchaseOrder.ExistSecurityTokens);

            acl = new DatabaseAccessControl(this.Security, localAdmin)[purchaseOrderItem];
            Assert.True(acl.CanRead(M.PurchaseOrderItem.Description));
            Assert.True(acl.CanWrite(M.PurchaseOrderItem.Description));
            Assert.True(acl.CanExecute(M.PurchaseOrderItem.Cancel));
        }

        [Fact]
        public void PurchaseOrderOtherInternalOrganisation()
        {
            var otherInternalOrganisation = new OrganisationBuilder(this.Transaction).WithIsInternalOrganisation(true).WithName("other internalOrganisation").Build();

            var contactMechanism = new PostalAddressBuilder(this.Transaction)
                .WithAddress1("address")
                .WithLocality("city")
                .WithCountry(new Countries(this.Transaction).FindBy(M.Country.IsoCode, "BE"))
                .Build();

            var supplier = new OrganisationBuilder(this.Transaction).WithName("other organisation").Build();
            new SupplierRelationshipBuilder(this.Transaction).WithInternalOrganisation(otherInternalOrganisation).WithSupplier(supplier).Build();

            this.Transaction.Derive();

            var localAdmin = new PersonBuilder(this.Transaction)
                .WithUserName("localadmin")
                .WithLastName("localAdmin")
                .Build();

            this.InternalOrganisation.AddLocalAdministrator(localAdmin);
            this.Transaction.Derive();

            this.Transaction.SetUser(localAdmin);

            var purchaseOrder = new PurchaseOrderBuilder(this.Transaction)
                .WithOrderedBy(otherInternalOrganisation)
                .WithAssignedBillToContactMechanism(contactMechanism)
                .WithTakenViaSupplier(supplier)
                .Build();

            this.Transaction.Derive();

            Assert.True(purchaseOrder.ExistSecurityTokens);

            var acl = new DatabaseAccessControl(this.Security, localAdmin)[purchaseOrder];
            Assert.False(acl.CanRead(M.PurchaseOrder.Description));
            Assert.False(acl.CanWrite(M.PurchaseOrder.Description));
        }

        [Fact]
        public void SalesOrderOwnInternalOrganisation()
        {
            var localAdmin = new PersonBuilder(this.Transaction)
                .WithUserName("localadmin")
                .WithLastName("localAdmin")
                .Build();

            this.InternalOrganisation.AddLocalAdministrator(localAdmin);
            this.Transaction.Derive();

            this.Transaction.SetUser(localAdmin);

            var salesOrder = new SalesOrderBuilder(this.Transaction).WithTakenBy(this.InternalOrganisation).WithBillToCustomer(this.Customer).Build();

            this.Transaction.Derive();

            Assert.True(salesOrder.Strategy.IsNewInTransaction);

            var acl = new DatabaseAccessControl(this.Security, localAdmin)[salesOrder];
            Assert.True(acl.CanRead(M.SalesOrder.Description));
            Assert.True(acl.CanWrite(M.SalesOrder.Description));

            this.Transaction.Commit();

            Assert.True(salesOrder.ExistSecurityTokens);

            Assert.False(salesOrder.Strategy.IsNewInTransaction);

            acl = new DatabaseAccessControl(this.Security, localAdmin)[salesOrder];
            Assert.True(acl.CanRead(M.SalesOrder.Description));
            Assert.True(acl.CanWrite(M.SalesOrder.Description));

            var salesOrderItem = new SalesOrderItemBuilder(this.Transaction).WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).Other).WithAssignedUnitPrice(1).Build();
            salesOrder.AddSalesOrderItem(salesOrderItem);

            this.Transaction.Derive();

            acl = new DatabaseAccessControl(this.Security, localAdmin)[salesOrderItem];
            Assert.True(acl.CanRead(M.SalesOrderItem.Description));
            Assert.True(acl.CanWrite(M.SalesOrderItem.Description));
            Assert.False(acl.CanRead(M.SalesOrderItem.CostOfGoodsSold));
        }

        [Fact]
        public void SalesOrderOtherInternalOrganisation()
        {
            var otherInternalOrganisation = new OrganisationBuilder(this.Transaction).WithIsInternalOrganisation(true).WithName("other internalOrganisation").Build();
            new StoreBuilder(this.Transaction)
                .WithName("other store")
                .WithInternalOrganisation(otherInternalOrganisation)
                .WithDefaultCarrier(this.Transaction.Extent<Carrier>().First())
                .WithDefaultCollectionMethod(this.Transaction.Extent<PaymentMethod>().First())
                .WithDefaultShipmentMethod(this.Transaction.Extent<ShipmentMethod>().First())
                .Build();

            var customer = new OrganisationBuilder(this.Transaction).WithName("other organisation").Build();
            new CustomerRelationshipBuilder(this.Transaction).WithCustomer(customer).WithInternalOrganisation(otherInternalOrganisation).Build();

            var contactMechanism = new PostalAddressBuilder(this.Transaction)
                .WithAddress1("address")
                .WithLocality("city")
                .WithCountry(new Countries(this.Transaction).FindBy(M.Country.IsoCode, "BE"))
                .Build();

            this.Transaction.Derive();

            var localAdmin = new PersonBuilder(this.Transaction)
                .WithUserName("localadmin")
                .WithLastName("localAdmin")
                .Build();

            this.InternalOrganisation.AddLocalAdministrator(localAdmin);
            this.Transaction.Derive();

            this.Transaction.SetUser(localAdmin);

            var salesOrder = new SalesOrderBuilder(this.Transaction)
                .WithTakenBy(otherInternalOrganisation)
                .WithBillToCustomer(customer)
                .WithAssignedTakenByContactMechanism(contactMechanism)
                .WithAssignedBillToContactMechanism(contactMechanism)
                .Build();

            this.Transaction.Derive();

            Assert.True(salesOrder.ExistSecurityTokens);

            var acl = new DatabaseAccessControl(this.Security, localAdmin)[salesOrder];
            Assert.False(acl.CanRead(M.SalesOrder.Description));
            Assert.False(acl.CanWrite(M.SalesOrder.Description));
        }

        [Fact]
        public void OwnInternalOrganisation()
        {
            var localAdmin = new PersonBuilder(this.Transaction)
                .WithUserName("localadmin")
                .WithLastName("localadmin")
                .Build();

            this.InternalOrganisation.AddLocalAdministrator(localAdmin);
            this.Transaction.Derive();

            this.Transaction.SetUser(localAdmin);

            var acl = new DatabaseAccessControl(this.Security, localAdmin)[this.InternalOrganisation];
            Assert.True(acl.CanExecute(M.InternalOrganisation.CreatePurchaseOrder));
            Assert.True(acl.CanExecute(M.InternalOrganisation.CreatePurchaseInvoice));
            Assert.True(acl.CanExecute(M.InternalOrganisation.CreateRequest));
            Assert.True(acl.CanExecute(M.InternalOrganisation.CreateQuote));
            Assert.True(acl.CanExecute(M.InternalOrganisation.CreateSalesInvoice));
            Assert.True(acl.CanExecute(M.InternalOrganisation.CreateSalesOrder));
            Assert.True(acl.CanRead(M.Organisation.Name));
            Assert.True(acl.CanWrite(M.Organisation.Name));
        }

        [Fact]
        public void OtherInternalOrganisation()
        {
            var localAdmin = new PersonBuilder(this.Transaction)
                .WithUserName("localadmin")
                .WithLastName("localadmin")
                .Build();

            this.InternalOrganisation.AddLocalAdministrator(localAdmin);

            var otherInternalOrganisation = new OrganisationBuilder(this.Transaction).WithIsInternalOrganisation(true).WithName("other internalOrganisation").Build();
            this.Transaction.Derive();

            this.Transaction.SetUser(localAdmin);

            var acl = new DatabaseAccessControl(this.Security, localAdmin)[otherInternalOrganisation];
            Assert.False(acl.CanExecute(M.InternalOrganisation.CreatePurchaseOrder));
            Assert.False(acl.CanExecute(M.InternalOrganisation.CreatePurchaseInvoice));
            Assert.False(acl.CanExecute(M.InternalOrganisation.CreateRequest));
            Assert.False(acl.CanExecute(M.InternalOrganisation.CreateQuote));
            Assert.False(acl.CanExecute(M.InternalOrganisation.CreateSalesInvoice));
            Assert.False(acl.CanExecute(M.InternalOrganisation.CreateSalesOrder));
            Assert.True(acl.CanRead(M.Organisation.Name));
            Assert.False(acl.CanWrite(M.Organisation.Name));
        }

        [Fact]
        public void OrganisationIsCustomerAtOwnInternalOrganisation()
        {
            var localAdmin = new PersonBuilder(this.Transaction)
                .WithUserName("localadmin")
                .WithLastName("localadmin")
                .Build();

            this.InternalOrganisation.AddLocalAdministrator(localAdmin);
            this.Transaction.Derive();

            this.Transaction.SetUser(localAdmin);

            var acl = new DatabaseAccessControl(this.Security, localAdmin)[this.Customer];
            Assert.True(acl.CanRead(M.Organisation.Name));
            Assert.True(acl.CanWrite(M.Organisation.Name));
            Assert.False(acl.CanExecute(M.Organisation.Delete));
        }

        [Fact]
        public void OrganisationIsCustomerAtOtherInternalOrganisation()
        {
            var otherInternalOrganisation = new OrganisationBuilder(this.Transaction).WithIsInternalOrganisation(true).WithName("other internalOrganisation").Build();
            var organisation = new OrganisationBuilder(this.Transaction).WithName("Org1").Build();
            new CustomerRelationshipBuilder(this.Transaction).WithCustomer(organisation).WithInternalOrganisation(otherInternalOrganisation).Build();

            this.Transaction.Derive();

            var localAdmin = new PersonBuilder(this.Transaction)
                .WithUserName("localadmin")
                .WithLastName("localadmin")
                .Build();

            this.InternalOrganisation.AddLocalAdministrator(localAdmin);
            this.Transaction.Derive();

            this.Transaction.SetUser(localAdmin);

            var acl = new DatabaseAccessControl(this.Security, localAdmin)[organisation];
            Assert.True(acl.CanRead(M.Organisation.Name));
            Assert.True(acl.CanWrite(M.Organisation.Name));
            Assert.False(acl.CanExecute(M.Organisation.Delete));
        }

        [Fact]
        public void SalesInvoiceOwnInternalOrganisation()
        {
            var localAdmin = new PersonBuilder(this.Transaction)
                .WithUserName("localadmin")
                .WithLastName("localAdmin")
                .Build();

            this.InternalOrganisation.AddLocalAdministrator(localAdmin);
            this.Transaction.Derive();

            this.Transaction.SetUser(localAdmin);

            var salesInvoice = new SalesInvoiceBuilder(this.Transaction).WithBilledFrom(this.InternalOrganisation).WithBillToCustomer(this.Customer).Build();

            this.Transaction.Derive();

            Assert.True(salesInvoice.ExistSecurityTokens);
            Assert.True(salesInvoice.Strategy.IsNewInTransaction);

            var acl = new DatabaseAccessControl(this.Security, localAdmin)[salesInvoice];
            Assert.True(acl.CanRead(M.SalesInvoice.Description));
            Assert.True(acl.CanWrite(M.SalesInvoice.Description));
            Assert.True(acl.CanExecute(M.SalesInvoice.Print));

            this.Transaction.Commit();

            Assert.False(salesInvoice.Strategy.IsNewInTransaction);

            acl = new DatabaseAccessControl(this.Security, localAdmin)[salesInvoice];
            Assert.True(acl.CanRead(M.SalesInvoice.Description));
            Assert.True(acl.CanWrite(M.SalesInvoice.Description));
            Assert.True(acl.CanExecute(M.SalesInvoice.Print));
        }

        [Fact]
        public void SalesInvoiceOtherInternalOrganisation()
        {
            var otherInternalOrganisation = new OrganisationBuilder(this.Transaction).WithIsInternalOrganisation(true).WithName("other internalOrganisation").Build();
            new StoreBuilder(this.Transaction)
                .WithName("other store")
                .WithInternalOrganisation(otherInternalOrganisation)
                .WithDefaultCarrier(this.Transaction.Extent<Carrier>().First())
                .WithDefaultCollectionMethod(this.Transaction.Extent<PaymentMethod>().First())
                .WithDefaultShipmentMethod(this.Transaction.Extent<ShipmentMethod>().First())
                .Build();

            var customer = new OrganisationBuilder(this.Transaction).WithName("other organisation").Build();
            new CustomerRelationshipBuilder(this.Transaction).WithCustomer(customer).WithInternalOrganisation(otherInternalOrganisation).Build();

            var contactMechanism = new PostalAddressBuilder(this.Transaction)
                .WithAddress1("address")
                .WithLocality("city")
                .WithCountry(new Countries(this.Transaction).FindBy(M.Country.IsoCode, "BE"))
                .Build();

            var localAdmin = new PersonBuilder(this.Transaction)
                .WithUserName("localadmin")
                .WithLastName("localAdmin")
                .Build();

            this.InternalOrganisation.AddLocalAdministrator(localAdmin);
            this.Transaction.Derive();

            this.Transaction.SetUser(localAdmin);

            var salesInvoice = new SalesInvoiceBuilder(this.Transaction).WithBilledFrom(otherInternalOrganisation).WithBillToCustomer(customer).WithAssignedBillToContactMechanism(contactMechanism).Build();

            this.Transaction.Derive();

            Assert.True(salesInvoice.ExistSecurityTokens);
            Assert.True(salesInvoice.Strategy.IsNewInTransaction);

            var acl = new DatabaseAccessControl(this.Security, localAdmin)[salesInvoice];
            Assert.False(acl.CanRead(M.SalesInvoice.Description));
            Assert.False(acl.CanWrite(M.SalesInvoice.Description));
            Assert.False(acl.CanExecute(M.SalesInvoice.Print));

            this.Transaction.Commit();

            Assert.False(salesInvoice.Strategy.IsNewInTransaction);

            acl = new DatabaseAccessControl(this.Security, localAdmin)[salesInvoice];
            Assert.False(acl.CanRead(M.SalesInvoice.Description));
            Assert.False(acl.CanWrite(M.SalesInvoice.Description));
            Assert.False(acl.CanExecute(M.SalesInvoice.Print));
        }

        [Fact]
        public void PurchaseInvoiceOwnInternalOrganisation()
        {
            var localAdmin = new PersonBuilder(this.Transaction)
                .WithUserName("localadmin")
                .WithLastName("localAdmin")
                .Build();

            this.InternalOrganisation.AddLocalAdministrator(localAdmin);
            this.Transaction.Derive();

            this.Transaction.SetUser(localAdmin);

            var purchaseInvoice = new PurchaseInvoiceBuilder(this.Transaction).WithBilledTo(this.InternalOrganisation).WithBilledFrom(this.Supplier).WithPurchaseInvoiceType(new PurchaseInvoiceTypes(this.Transaction).PurchaseInvoice).Build();

            this.Transaction.Derive();

            Assert.True(purchaseInvoice.ExistSecurityTokens);
            Assert.True(purchaseInvoice.Strategy.IsNewInTransaction);

            var acl = new DatabaseAccessControl(this.Security, localAdmin)[purchaseInvoice];
            Assert.True(acl.CanRead(M.PurchaseInvoice.Description));
            Assert.True(acl.CanWrite(M.PurchaseInvoice.Description));
            Assert.True(acl.CanExecute(M.PurchaseInvoice.Print));

            this.Transaction.Commit();

            Assert.False(purchaseInvoice.Strategy.IsNewInTransaction);

            acl = new DatabaseAccessControl(this.Security, localAdmin)[purchaseInvoice];
            Assert.True(acl.CanRead(M.PurchaseInvoice.Description));
            Assert.True(acl.CanWrite(M.PurchaseInvoice.Description));
            Assert.True(acl.CanExecute(M.PurchaseInvoice.Print));
        }

        [Fact]
        public void PurchaseInvoiceOtherInternalOrganisation()
        {
            var otherInternalOrganisation = new OrganisationBuilder(this.Transaction).WithIsInternalOrganisation(true).WithName("other internalOrganisation").Build();

            var supplier = new OrganisationBuilder(this.Transaction).WithName("other organisation").Build();
            new SupplierRelationshipBuilder(this.Transaction).WithInternalOrganisation(otherInternalOrganisation).WithSupplier(supplier).Build();

            var purchaseInvoice = new PurchaseInvoiceBuilder(this.Transaction).WithBilledTo(otherInternalOrganisation).WithBilledFrom(supplier).WithPurchaseInvoiceType(new PurchaseInvoiceTypes(this.Transaction).PurchaseInvoice).Build();

            this.Transaction.Derive();

            var localAdmin = new PersonBuilder(this.Transaction)
                .WithUserName("localadmin")
                .WithLastName("localAdmin")
                .Build();

            this.InternalOrganisation.AddLocalAdministrator(localAdmin);
            this.Transaction.Derive();

            this.Transaction.SetUser(localAdmin);

            Assert.True(purchaseInvoice.ExistSecurityTokens);
            Assert.True(purchaseInvoice.Strategy.IsNewInTransaction);

            var acl = new DatabaseAccessControl(this.Security, localAdmin)[purchaseInvoice];
            Assert.False(acl.CanRead(M.PurchaseInvoice.Description));
            Assert.False(acl.CanWrite(M.PurchaseInvoice.Description));
            Assert.False(acl.CanExecute(M.PurchaseInvoice.Print));

            this.Transaction.Commit();

            Assert.False(purchaseInvoice.Strategy.IsNewInTransaction);

            acl = new DatabaseAccessControl(this.Security, localAdmin)[purchaseInvoice];
            Assert.False(acl.CanRead(M.PurchaseInvoice.Description));
            Assert.False(acl.CanWrite(M.PurchaseInvoice.Description));
            Assert.False(acl.CanExecute(M.PurchaseInvoice.Print));
        }

        [Fact]
        public void CustomerShipmentOwnInternalOrganisation()
        {
            var shipment = new CustomerShipmentBuilder(this.Transaction).WithShipFromParty(this.InternalOrganisation).WithShipToParty(this.Customer).Build();

            this.Transaction.Derive();

            var localAdmin = new PersonBuilder(this.Transaction)
                .WithUserName("localadmin")
                .WithLastName("localAdmin")
                .Build();

            this.InternalOrganisation.AddLocalAdministrator(localAdmin);
            this.Transaction.Derive();

            this.Transaction.SetUser(localAdmin);

            Assert.True(shipment.ExistSecurityTokens);
            
            var acl = new DatabaseAccessControl(this.Security, localAdmin)[shipment];
            Assert.True(acl.CanRead(M.CustomerShipment.Comment));
            Assert.True(acl.CanWrite(M.CustomerShipment.Comment));
            Assert.True(acl.CanExecute(M.CustomerShipment.Cancel));
        }

        [Fact]
        public void CustomerShipmentOtherInternalOrganisation()
        {
            var otherInternalOrganisation = new OrganisationBuilder(this.Transaction).WithIsInternalOrganisation(true).WithName("other internalOrganisation").Build();
            new StoreBuilder(this.Transaction)
                .WithName("other store")
                .WithInternalOrganisation(otherInternalOrganisation)
                .WithDefaultCarrier(this.Transaction.Extent<Carrier>().First())
                .WithDefaultCollectionMethod(this.Transaction.Extent<PaymentMethod>().First())
                .WithDefaultShipmentMethod(this.Transaction.Extent<ShipmentMethod>().First())
                .Build();

            var organisation = new OrganisationBuilder(this.Transaction).WithName("other organisation").Build();
            new CustomerRelationshipBuilder(this.Transaction).WithInternalOrganisation(otherInternalOrganisation).WithCustomer(organisation).Build();

            var shipment = new CustomerShipmentBuilder(this.Transaction).WithShipFromParty(otherInternalOrganisation).WithShipToParty(organisation).Build();

            this.Transaction.Derive();

            var localAdmin = new PersonBuilder(this.Transaction)
                .WithUserName("localadmin")
                .WithLastName("localAdmin")
                .Build();

            this.InternalOrganisation.AddLocalAdministrator(localAdmin);
            this.Transaction.Derive();

            this.Transaction.SetUser(localAdmin);

            Assert.True(shipment.ExistSecurityTokens);

            var acl = new DatabaseAccessControl(this.Security, localAdmin)[shipment];
            Assert.False(acl.CanRead(M.CustomerShipment.Comment));
            Assert.False(acl.CanWrite(M.CustomerShipment.Comment));
            Assert.False(acl.CanExecute(M.CustomerShipment.Cancel));
        }

        [Fact]
        public void RequestForQuoteOwnInternalOrganisation()
        {
            var request = new RequestForQuoteBuilder(this.Transaction).WithRecipient(this.InternalOrganisation).WithOriginator(this.Customer).Build();

            this.Transaction.Derive();

            var localAdmin = new PersonBuilder(this.Transaction)
                .WithUserName("localadmin")
                .WithLastName("localAdmin")
                .Build();

            this.InternalOrganisation.AddLocalAdministrator(localAdmin);
            this.Transaction.Derive();

            this.Transaction.SetUser(localAdmin);

            Assert.True(request.ExistSecurityTokens);

            var acl = new DatabaseAccessControl(this.Security, localAdmin)[request];
            Assert.True(acl.CanRead(M.RequestForQuote.Comment));
            Assert.True(acl.CanWrite(M.RequestForQuote.Comment));
            Assert.True(acl.CanExecute(M.RequestForQuote.Cancel));
        }

        [Fact]
        public void RequestForQuoteOtherInternalOrganisation()
        {
            var otherInternalOrganisation = new OrganisationBuilder(this.Transaction).WithIsInternalOrganisation(true).WithName("other internalOrganisation").WithRequestSequence(new RequestSequences(this.Transaction).EnforcedSequence).Build();
            new StoreBuilder(this.Transaction)
                .WithName("other store")
                .WithInternalOrganisation(otherInternalOrganisation)
                .WithDefaultCarrier(this.Transaction.Extent<Carrier>().First())
                .WithDefaultCollectionMethod(this.Transaction.Extent<PaymentMethod>().First())
                .WithDefaultShipmentMethod(this.Transaction.Extent<ShipmentMethod>().First())
                .Build();

            var organisation = new OrganisationBuilder(this.Transaction).WithName("other organisation").Build();
            new CustomerRelationshipBuilder(this.Transaction).WithInternalOrganisation(otherInternalOrganisation).WithCustomer(organisation).Build();

            var request = new RequestForQuoteBuilder(this.Transaction).WithRecipient(otherInternalOrganisation).WithOriginator(organisation).Build();

            this.Transaction.Derive();

            var localAdmin = new PersonBuilder(this.Transaction)
                .WithUserName("localadmin")
                .WithLastName("localAdmin")
                .Build();

            this.InternalOrganisation.AddLocalAdministrator(localAdmin);
            this.Transaction.Derive();

            this.Transaction.SetUser(localAdmin);

            Assert.True(request.ExistSecurityTokens);

            var acl = new DatabaseAccessControl(this.Security, localAdmin)[request];
            Assert.False(acl.CanRead(M.RequestForQuote.Comment));
            Assert.False(acl.CanWrite(M.RequestForQuote.Comment));
            Assert.False(acl.CanExecute(M.RequestForQuote.Cancel));
        }

        [Fact]
        public void ProductQuoteOwnInternalOrganisation()
        {
            var contactMechanism = new PostalAddressBuilder(this.Transaction)
               .WithAddress1("address")
               .WithLocality("city")
               .WithCountry(new Countries(this.Transaction).FindBy(M.Country.IsoCode, "BE"))
               .Build();

            var quote = new ProductQuoteBuilder(this.Transaction).WithIssuer(this.InternalOrganisation).WithReceiver(this.Customer).WithFullfillContactMechanism(contactMechanism).Build();

            this.Transaction.Derive();

            var localAdmin = new PersonBuilder(this.Transaction)
                .WithUserName("localadmin")
                .WithLastName("localAdmin")
                .Build();

            this.InternalOrganisation.AddLocalAdministrator(localAdmin);
            this.Transaction.Derive();

            this.Transaction.SetUser(localAdmin);

            Assert.True(quote.ExistSecurityTokens);

            var acl = new DatabaseAccessControl(this.Security, localAdmin)[quote];
            Assert.True(acl.CanRead(M.ProductQuote.Comment));
            Assert.True(acl.CanWrite(M.ProductQuote.Comment));
            Assert.True(acl.CanExecute(M.ProductQuote.Cancel));
        }

        [Fact]
        public void ProductQuoteOtherInternalOrganisation()
        {
            var contactMechanism = new PostalAddressBuilder(this.Transaction)
              .WithAddress1("address")
              .WithLocality("city")
              .WithCountry(new Countries(this.Transaction).FindBy(M.Country.IsoCode, "BE"))
              .Build();

            var otherInternalOrganisation = new OrganisationBuilder(this.Transaction).WithIsInternalOrganisation(true).WithName("other internalOrganisation").WithQuoteSequence(new QuoteSequences(this.Transaction).EnforcedSequence).Build();
            new StoreBuilder(this.Transaction)
                .WithName("other store")
                .WithInternalOrganisation(otherInternalOrganisation)
                .WithDefaultCarrier(this.Transaction.Extent<Carrier>().First())
                .WithDefaultCollectionMethod(this.Transaction.Extent<PaymentMethod>().First())
                .WithDefaultShipmentMethod(this.Transaction.Extent<ShipmentMethod>().First())
                .Build();

            var organisation = new OrganisationBuilder(this.Transaction).WithName("other organisation").Build();
            new CustomerRelationshipBuilder(this.Transaction).WithInternalOrganisation(otherInternalOrganisation).WithCustomer(organisation).Build();

            var quote = new ProductQuoteBuilder(this.Transaction).WithIssuer(otherInternalOrganisation).WithReceiver(organisation).WithFullfillContactMechanism(contactMechanism).Build();

            this.Transaction.Derive();

            var localAdmin = new PersonBuilder(this.Transaction)
                .WithUserName("localadmin")
                .WithLastName("localAdmin")
                .Build();

            this.InternalOrganisation.AddLocalAdministrator(localAdmin);
            this.Transaction.Derive();

            this.Transaction.SetUser(localAdmin);

            Assert.True(quote.ExistSecurityTokens);

            var acl = new DatabaseAccessControl(this.Security, localAdmin)[quote];
            Assert.False(acl.CanRead(M.ProductQuote.Comment));
            Assert.False(acl.CanWrite(M.ProductQuote.Comment));
            Assert.False(acl.CanExecute(M.ProductQuote.Cancel));
        }

        [Fact]
        public void InventoryItemTransactionOwnInventory()
        {
            var part = new UnifiedGoodBuilder(this.Transaction).WithName("name").WithInventoryItemKind(new InventoryItemKinds(this.Transaction).NonSerialised).Build();
            this.Transaction.Derive();

            var inventoryItemTransaction = new InventoryItemTransactionBuilder(this.Transaction)
                .WithPart(part)
                .WithReason(new InventoryTransactionReasons(this.Transaction).IncomingShipment)
                .WithFacility(this.InternalOrganisation.FacilitiesWhereOwner.First())
                .WithQuantity(2)
                .Build();

            this.Transaction.Derive();

            var localAdmin = new PersonBuilder(this.Transaction)
                .WithUserName("localadmin")
                .WithLastName("localAdmin")
                .Build();

            this.InternalOrganisation.AddLocalAdministrator(localAdmin);
            this.Transaction.Derive();

            this.Transaction.SetUser(localAdmin);

            Assert.True(inventoryItemTransaction.ExistSecurityTokens);

            var acl = new DatabaseAccessControl(this.Security, localAdmin)[inventoryItemTransaction];
            Assert.True(acl.CanRead(M.InventoryItemTransaction.Quantity));
            Assert.True(acl.CanWrite(M.InventoryItemTransaction.Quantity));
        }

        [Fact]
        public void InventoryItemTransactionOtherInventory()
        {
            var otherInternalOrganisation = new OrganisationBuilder(this.Transaction).WithIsInternalOrganisation(true).WithName("other internalOrganisation").Build();
            var otherFacility = new FacilityBuilder(this.Transaction).WithName("other facility").WithOwner(otherInternalOrganisation).WithFacilityType(new FacilityTypes(this.Transaction).Warehouse).Build();

            var part = new UnifiedGoodBuilder(this.Transaction).WithName("name").WithInventoryItemKind(new InventoryItemKinds(this.Transaction).NonSerialised).Build();
            this.Transaction.Derive();

            var inventoryItemTransaction = new InventoryItemTransactionBuilder(this.Transaction)
                .WithPart(part)
                .WithReason(new InventoryTransactionReasons(this.Transaction).IncomingShipment)
                .WithFacility(otherFacility)
                .WithQuantity(2)
                .Build();

            this.Transaction.Derive();

            var localAdmin = new PersonBuilder(this.Transaction)
                .WithUserName("localadmin")
                .WithLastName("localAdmin")
                .Build();

            this.InternalOrganisation.AddLocalAdministrator(localAdmin);
            this.Transaction.Derive();

            this.Transaction.SetUser(localAdmin);

            Assert.True(inventoryItemTransaction.ExistSecurityTokens);

            var acl = new DatabaseAccessControl(this.Security, localAdmin)[inventoryItemTransaction];
            Assert.False(acl.CanRead(M.InventoryItemTransaction.Quantity));
            Assert.False(acl.CanWrite(M.InventoryItemTransaction.Quantity));
        }

        [Fact]
        public void WorkRequirementOwnInternalOrganisation()
        {
            var workRequirement = new WorkRequirementBuilder(this.Transaction)
                .WithServicedBy(this.InternalOrganisation)
                .WithLocation("location")
                .WithReason("reason")
                .WithDescription("description")
                .WithFixedAsset(new SerialisedItemBuilder(this.Transaction).Build())
                .Build();

            this.Transaction.Derive();

            var localAdmin = new PersonBuilder(this.Transaction)
                .WithUserName("localadmin")
                .WithLastName("localAdmin")
                .Build();

            this.InternalOrganisation.AddLocalAdministrator(localAdmin);
            this.Transaction.Derive();

            this.Transaction.SetUser(localAdmin);

            Assert.True(workRequirement.ExistSecurityTokens);

            var acl = new DatabaseAccessControl(this.Security, localAdmin)[workRequirement];
            Assert.True(acl.CanRead(M.WorkRequirement.Description));
            Assert.True(acl.CanWrite(M.WorkRequirement.Description));
            Assert.True(acl.CanExecute(M.WorkRequirement.Cancel));
        }

        [Fact]
        public void WorkRequirementOtherInternalOrganisation()
        {
            var otherInternalOrganisation = new OrganisationBuilder(this.Transaction).WithIsInternalOrganisation(true).WithName("other internalOrganisation").WithRequirementSequence(new RequirementSequences(this.Transaction).EnforcedSequence).Build();

            var workRequirement = new WorkRequirementBuilder(this.Transaction)
                .WithServicedBy(otherInternalOrganisation)
                .WithLocation("location")
                .WithReason("reason")
                .WithDescription("description")
                .WithFixedAsset(new SerialisedItemBuilder(this.Transaction).Build())
                .Build();

            this.Transaction.Derive();

            var localAdmin = new PersonBuilder(this.Transaction)
                .WithUserName("localadmin")
                .WithLastName("localAdmin")
                .Build();

            this.InternalOrganisation.AddLocalAdministrator(localAdmin);
            this.Transaction.Derive();

            this.Transaction.SetUser(localAdmin);

            Assert.True(workRequirement.ExistSecurityTokens);

            var acl = new DatabaseAccessControl(this.Security, localAdmin)[workRequirement];
            Assert.False(acl.CanRead(M.WorkRequirement.Description));
            Assert.False(acl.CanWrite(M.WorkRequirement.Description));
            Assert.False(acl.CanExecute(M.WorkRequirement.Cancel));
        }

        [Fact]
        public void WorkRequirementFulfillmentOwnInternalOrganisation()
        {
            var workRequirement = new WorkRequirementBuilder(this.Transaction)
                .WithServicedBy(this.InternalOrganisation)
                .WithLocation("location")
                .WithReason("reason")
                .WithDescription("description")
                .WithFixedAsset(new SerialisedItemBuilder(this.Transaction).Build())
                .Build();

            var workTask = new WorkTaskBuilder(this.Transaction).WithName("Activity").WithCustomer(this.Customer).WithTakenBy(this.InternalOrganisation).Build();

            var fufillment = new WorkRequirementFulfillmentBuilder(this.Transaction).WithFullfillmentOf(workTask).WithFullfilledBy(workRequirement).Build();

            this.Transaction.Derive();

            var localAdmin = new PersonBuilder(this.Transaction)
                .WithUserName("localadmin")
                .WithLastName("localAdmin")
                .Build();

            this.InternalOrganisation.AddLocalAdministrator(localAdmin);
            this.Transaction.Derive();

            this.Transaction.SetUser(localAdmin);

            Assert.True(fufillment.ExistSecurityTokens);

            var acl = new DatabaseAccessControl(this.Security, localAdmin)[fufillment];
            Assert.True(acl.CanRead(M.WorkRequirementFulfillment.FullfilledBy));
            Assert.True(acl.CanWrite(M.WorkRequirementFulfillment.FullfilledBy));
            Assert.True(acl.CanExecute(M.WorkRequirementFulfillment.Delete));
        }

        [Fact]
        public void WorkRequirementFulfillmentOtherInternalOrganisation()
        {
            var otherInternalOrganisation = new OrganisationBuilder(this.Transaction).WithIsInternalOrganisation(true).WithName("other internalOrganisation").WithRequirementSequence(new RequirementSequences(this.Transaction).EnforcedSequence).Build();

            this.Transaction.Derive();

            var workRequirement = new WorkRequirementBuilder(this.Transaction)
                .WithServicedBy(otherInternalOrganisation)
                .WithLocation("location")
                .WithReason("reason")
                .WithDescription("description")
                .WithFixedAsset(new SerialisedItemBuilder(this.Transaction).Build())
                .Build();

            var workTask = new WorkTaskBuilder(this.Transaction).WithName("Activity").WithCustomer(this.Customer).WithTakenBy(otherInternalOrganisation).Build();

            var fufillment = new WorkRequirementFulfillmentBuilder(this.Transaction).WithFullfillmentOf(workTask).WithFullfilledBy(workRequirement).Build();

            this.Transaction.Derive();

            var localAdmin = new PersonBuilder(this.Transaction)
                .WithUserName("localadmin")
                .WithLastName("localAdmin")
                .Build();

            this.InternalOrganisation.AddLocalAdministrator(localAdmin);
            this.Transaction.Derive();

            this.Transaction.SetUser(localAdmin);

            Assert.True(fufillment.ExistSecurityTokens);

            var acl = new DatabaseAccessControl(this.Security, localAdmin)[fufillment];
            Assert.False(acl.CanRead(M.WorkRequirementFulfillment.FullfilledBy));
            Assert.False(acl.CanWrite(M.WorkRequirementFulfillment.FullfilledBy));
            Assert.False(acl.CanExecute(M.WorkRequirementFulfillment.Delete));
        }

        [Fact]
        public void SupplierOfferingOwnInternalOrganisation()
        {
            var part = new NonUnifiedPartBuilder(this.Transaction).WithName("name").Build();
            var offering = new SupplierOfferingBuilder(this.Transaction).WithPart(part).WithSupplier(this.Supplier).WithPrice(1).WithUnitOfMeasure(new UnitsOfMeasure(this.Transaction).Piece).Build();

            this.Transaction.Derive();

            var localAdmin = new PersonBuilder(this.Transaction)
                .WithUserName("localadmin")
                .WithLastName("localAdmin")
                .Build();

            this.InternalOrganisation.AddLocalAdministrator(localAdmin);
            this.Transaction.Derive();

            this.Transaction.SetUser(localAdmin);

            Assert.True(offering.ExistSecurityTokens);

            var acl = new DatabaseAccessControl(this.Security, localAdmin)[offering];
            Assert.True(acl.CanRead(M.SupplierOffering.Price));
            Assert.True(acl.CanWrite(M.SupplierOffering.Price));
        }

        [Fact]
        public void SupplierOfferingOtherInternalOrganisation()
        {
            var otherInternalOrganisation = new OrganisationBuilder(this.Transaction).WithIsInternalOrganisation(true).WithName("other internalOrganisation").Build();
            var part = new NonUnifiedPartBuilder(this.Transaction).WithName("name").Build();
            var supplier = new OrganisationBuilder(this.Transaction).WithName("other organisation").Build();
            new SupplierRelationshipBuilder(this.Transaction).WithInternalOrganisation(otherInternalOrganisation).WithSupplier(supplier).Build();

            var offering = new SupplierOfferingBuilder(this.Transaction).WithPart(part).WithSupplier(supplier).WithPrice(1).WithUnitOfMeasure(new UnitsOfMeasure(this.Transaction).Piece).Build();

            this.Transaction.Derive();

            var localAdmin = new PersonBuilder(this.Transaction)
                .WithUserName("localadmin")
                .WithLastName("localAdmin")
                .Build();

            this.InternalOrganisation.AddLocalAdministrator(localAdmin);
            this.Transaction.Derive();

            this.Transaction.SetUser(localAdmin);

            Assert.True(offering.ExistSecurityTokens);

            var acl = new DatabaseAccessControl(this.Security, localAdmin)[offering];
            Assert.False(acl.CanRead(M.SupplierOffering.Price));
            Assert.False(acl.CanWrite(M.SupplierOffering.Price));
        }

        [Fact]
        public void PersonIsContactForOwnCustomerOrganisation()
        {
            var localAdmin = new PersonBuilder(this.Transaction)
                .WithUserName("localadmin")
                .WithLastName("localadmin")
                .Build();

            this.InternalOrganisation.AddLocalAdministrator(localAdmin);
            this.Transaction.Derive();

            this.Transaction.SetUser(localAdmin);

            var contact = this.Customer.OrganisationContactRelationshipsWhereOrganisation.First().Contact;

            Assert.True(contact.ExistSecurityTokens);

            var acl = new DatabaseAccessControl(this.Security, localAdmin)[contact];
            Assert.True(acl.CanRead(M.Person.FirstName));
            Assert.True(acl.CanWrite(M.Person.FirstName));
            Assert.False(acl.CanRead(M.Person.UserPasswordHash));
            Assert.False(acl.CanWrite(M.Person.UserPasswordHash));
            Assert.True(acl.CanExecute(M.Person.Delete));
        }

        [Fact]
        public void PersonIsContactForOtherCustomerOrganisation()
        {
            var otherInternalOrganisation = new OrganisationBuilder(this.Transaction).WithIsInternalOrganisation(true).WithName("other internalOrganisation").Build();
            otherInternalOrganisation.CreateB2BCustomer(new Bogus.Faker());

            this.Transaction.Derive();

            var localAdmin = new PersonBuilder(this.Transaction)
                .WithUserName("localadmin")
                .WithLastName("localadmin")
                .Build();

            this.InternalOrganisation.AddLocalAdministrator(localAdmin);
            this.Transaction.Derive();

            this.Transaction.SetUser(localAdmin);

            var otherCustomer = (Organisation)otherInternalOrganisation.ActiveCustomers.First(v => v.GetType().Name.Equals(typeof(Organisation).Name));
            var contact = otherCustomer.OrganisationContactRelationshipsWhereOrganisation.First().Contact;

            Assert.True(contact.ExistSecurityTokens);

            var acl = new DatabaseAccessControl(this.Security, localAdmin)[contact];
            Assert.True(acl.CanRead(M.Person.FirstName));
            Assert.True(acl.CanWrite(M.Person.FirstName));
            Assert.False(acl.CanRead(M.Person.UserPasswordHash));
            Assert.False(acl.CanWrite(M.Person.UserPasswordHash));
            Assert.True(acl.CanExecute(M.Person.Delete));
        }

        [Fact]
        public void NonUnifiedPartOwnInternalOrganisation()
        {
            var localAdmin = new PersonBuilder(this.Transaction)
                .WithUserName("localadmin")
                .WithLastName("localAdmin")
                .Build();

            this.InternalOrganisation.AddLocalAdministrator(localAdmin);
            this.Transaction.Derive();

            this.Transaction.SetUser(localAdmin);

            var part = new NonUnifiedPartBuilder(this.Transaction).WithName("Name").WithDefaultFacility(this.DefaultFacility).Build();

            this.Transaction.Derive();

            Assert.True(part.ExistSecurityTokens);

            var acl = new DatabaseAccessControl(this.Security, localAdmin)[part];
            Assert.True(acl.CanRead(M.NonUnifiedPart.PartSpecifications));
            Assert.True(acl.CanWrite(M.NonUnifiedPart.PartSpecifications));
            Assert.True(acl.CanExecute(M.NonUnifiedPart.Delete));
        }

        [Fact]
        public void NonUnifiedPartOtherInternalOrganisation()
        {
            var otherInternalOrganisation = new OrganisationBuilder(this.Transaction).WithIsInternalOrganisation(true).WithName("other internalOrganisation").WithRequestSequence(new RequestSequences(this.Transaction).EnforcedSequence).Build();
            var otherFacility = new FacilityBuilder(this.Transaction).WithName("other facility").WithOwner(otherInternalOrganisation).WithFacilityType(new FacilityTypes(this.Transaction).Warehouse).Build();

            var part = new NonUnifiedPartBuilder(this.Transaction).WithName("Name").WithDefaultFacility(otherFacility).Build();

            this.Transaction.Derive();

            var localAdmin = new PersonBuilder(this.Transaction)
                .WithUserName("localadmin")
                .WithLastName("localAdmin")
                .Build();

            this.InternalOrganisation.AddLocalAdministrator(localAdmin);
            this.Transaction.Derive();

            this.Transaction.SetUser(localAdmin);

            Assert.True(part.ExistSecurityTokens);

            var acl = new DatabaseAccessControl(this.Security, localAdmin)[part];
            Assert.True(acl.CanRead(M.NonUnifiedPart.PartSpecifications));
            Assert.True(acl.CanWrite(M.NonUnifiedPart.PartSpecifications));
            Assert.True(acl.CanExecute(M.NonUnifiedPart.Delete));
        }

        [Fact]
        public void FacilityOwnInternalOrganisation()
        {
            var localAdmin = new PersonBuilder(this.Transaction)
                .WithUserName("localadmin")
                .WithLastName("localAdmin")
                .Build();

            this.InternalOrganisation.AddLocalAdministrator(localAdmin);
            this.Transaction.Derive();

            this.Transaction.SetUser(localAdmin);

            var facility = new FacilityBuilder(this.Transaction).WithName("facility").WithOwner(this.InternalOrganisation).WithFacilityType(new FacilityTypes(this.Transaction).Warehouse).Build();

            this.Transaction.Derive();

            Assert.True(facility.ExistSecurityTokens);

            var acl = new DatabaseAccessControl(this.Security, localAdmin)[facility];
            Assert.True(acl.CanRead(M.Facility.Name));
            Assert.True(acl.CanWrite(M.Facility.Name));
        }

        [Fact]
        public void FacilityOtherInternalOrganisation()
        {
            var otherInternalOrganisation = new OrganisationBuilder(this.Transaction).WithIsInternalOrganisation(true).WithName("other internalOrganisation").WithRequestSequence(new RequestSequences(this.Transaction).EnforcedSequence).Build();
            this.Transaction.Derive();

            var facility = new FacilityBuilder(this.Transaction).WithName("other facility").WithOwner(otherInternalOrganisation).WithFacilityType(new FacilityTypes(this.Transaction).Warehouse).Build();
            this.Transaction.Derive();

            var localAdmin = new PersonBuilder(this.Transaction)
                .WithUserName("localadmin")
                .WithLastName("localAdmin")
                .Build();

            this.InternalOrganisation.AddLocalAdministrator(localAdmin);
            this.Transaction.Derive();

            this.Transaction.SetUser(localAdmin);

            Assert.True(facility.ExistSecurityTokens);

            var acl = new DatabaseAccessControl(this.Security, localAdmin)[facility];
            Assert.True(acl.CanRead(M.Facility.Name));
            Assert.True(acl.CanWrite(M.Facility.Name));
        }

        [Fact]
        public void SupplierRelationshipForOwnCustomerOrganisation()
        {
            var localAdmin = new PersonBuilder(this.Transaction)
                .WithUserName("localadmin")
                .WithLastName("localadmin")
                .Build();

            this.InternalOrganisation.AddLocalAdministrator(localAdmin);
            this.Transaction.Derive();

            this.Transaction.SetUser(localAdmin);

            var relationship = this.Supplier.SupplierRelationshipsWhereSupplier.First();

            Assert.True(relationship.ExistSecurityTokens);

            var acl = new DatabaseAccessControl(this.Security, localAdmin)[relationship];
            Assert.True(acl.CanRead(M.SupplierRelationship.NeedsApproval));
            Assert.True(acl.CanWrite(M.SupplierRelationship.NeedsApproval));
            Assert.True(acl.CanExecute(M.SupplierRelationship.Delete));
        }

        [Fact]
        public void SupplierRelationshipForOtherCustomerOrganisation()
        {
            var otherInternalOrganisation = new OrganisationBuilder(this.Transaction).WithIsInternalOrganisation(true).WithName("other internalOrganisation").Build();
            otherInternalOrganisation.CreateSupplier(new Bogus.Faker());

            this.Transaction.Derive();

            var localAdmin = new PersonBuilder(this.Transaction)
                .WithUserName("localadmin")
                .WithLastName("localadmin")
                .Build();

            this.InternalOrganisation.AddLocalAdministrator(localAdmin);
            this.Transaction.Derive();

            this.Transaction.SetUser(localAdmin);

            var relationship = otherInternalOrganisation.SupplierRelationshipsWhereInternalOrganisation.First();

            Assert.True(relationship.ExistSecurityTokens);

            var acl = new DatabaseAccessControl(this.Security, localAdmin)[relationship];
            Assert.False(acl.CanRead(M.SupplierRelationship.NeedsApproval));
            Assert.False(acl.CanWrite(M.SupplierRelationship.NeedsApproval));
            Assert.False(acl.CanExecute(M.SupplierRelationship.Delete));
        }

        [Fact]
        public void SubContractorRelationshipForOwnCustomerOrganisation()
        {
            var localAdmin = new PersonBuilder(this.Transaction)
                .WithUserName("localadmin")
                .WithLastName("localadmin")
                .Build();

            this.InternalOrganisation.AddLocalAdministrator(localAdmin);
            this.Transaction.Derive();

            this.Transaction.SetUser(localAdmin);

            var relationship = this.SubContractor.SubContractorRelationshipsWhereSubContractor.First();

            Assert.True(relationship.ExistSecurityTokens);

            var acl = new DatabaseAccessControl(this.Security, localAdmin)[relationship];
            Assert.True(acl.CanRead(M.SubContractorRelationship.Agreements));
            Assert.True(acl.CanWrite(M.SubContractorRelationship.Agreements));
            Assert.True(acl.CanExecute(M.SubContractorRelationship.Delete));
        }

        [Fact]
        public void SubContractorRelationshipForOtherCustomerOrganisation()
        {
            var otherInternalOrganisation = new OrganisationBuilder(this.Transaction).WithIsInternalOrganisation(true).WithName("other internalOrganisation").Build();
            otherInternalOrganisation.CreateSubContractor(new Bogus.Faker());

            this.Transaction.Derive();

            var localAdmin = new PersonBuilder(this.Transaction)
                .WithUserName("localadmin")
                .WithLastName("localadmin")
                .Build();

            this.InternalOrganisation.AddLocalAdministrator(localAdmin);
            this.Transaction.Derive();

            this.Transaction.SetUser(localAdmin);

            var relationship = otherInternalOrganisation.SubContractorRelationshipsWhereContractor.First();

            Assert.True(relationship.ExistSecurityTokens);

            var acl = new DatabaseAccessControl(this.Security, localAdmin)[relationship];
            Assert.False(acl.CanRead(M.SubContractorRelationship.Agreements));
            Assert.False(acl.CanWrite(M.SubContractorRelationship.Agreements));
            Assert.False(acl.CanExecute(M.SubContractorRelationship.Delete));
        }

        [Fact]
        public void CustomerRelationshipForOwnCustomerOrganisation()
        {
            var localAdmin = new PersonBuilder(this.Transaction)
                .WithUserName("localadmin")
                .WithLastName("localadmin")
                .Build();

            this.InternalOrganisation.AddLocalAdministrator(localAdmin);
            this.Transaction.Derive();

            this.Transaction.SetUser(localAdmin);

            var relationship = this.Customer.CustomerRelationshipsWhereCustomer.First();

            Assert.True(relationship.ExistSecurityTokens);

            var acl = new DatabaseAccessControl(this.Security, localAdmin)[relationship];
            Assert.True(acl.CanRead(M.CustomerRelationship.Agreements));
            Assert.True(acl.CanWrite(M.CustomerRelationship.Agreements));
            Assert.True(acl.CanExecute(M.CustomerRelationship.Delete));
        }

        [Fact]
        public void CustomerRelationshipForOtherCustomerOrganisation()
        {
            var otherInternalOrganisation = new OrganisationBuilder(this.Transaction).WithIsInternalOrganisation(true).WithName("other internalOrganisation").Build();
            otherInternalOrganisation.CreateB2BCustomer(new Bogus.Faker());

            this.Transaction.Derive();

            var localAdmin = new PersonBuilder(this.Transaction)
                .WithUserName("localadmin")
                .WithLastName("localadmin")
                .Build();

            this.InternalOrganisation.AddLocalAdministrator(localAdmin);
            this.Transaction.Derive();

            this.Transaction.SetUser(localAdmin);

            var relationship = otherInternalOrganisation.CustomerRelationshipsWhereInternalOrganisation.First();

            Assert.True(relationship.ExistSecurityTokens);

            var acl = new DatabaseAccessControl(this.Security, localAdmin)[relationship];
            Assert.False(acl.CanRead(M.CustomerRelationship.Agreements));
            Assert.False(acl.CanWrite(M.CustomerRelationship.Agreements));
            Assert.False(acl.CanExecute(M.CustomerRelationship.Delete));
        }

        [Fact]
        public void ExchangeRate()
        {
            var localAdmin = new PersonBuilder(this.Transaction)
                .WithUserName("localadmin")
                .WithLastName("localAdmin")
                .Build();

            this.InternalOrganisation.AddLocalAdministrator(localAdmin);

            var exchangeRate = new ExchangeRateBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);
            this.Transaction.Commit();

            this.Transaction.SetUser(localAdmin);

            var acl = new DatabaseAccessControl(this.Security, localAdmin)[exchangeRate];
            Assert.True(acl.CanRead(M.ExchangeRate.Rate));
            Assert.False(acl.CanWrite(M.ExchangeRate.Rate));
        }

        [Fact]
        public void Settings()
        {
            var localAdmin = new PersonBuilder(this.Transaction)
                .WithUserName("localadmin")
                .WithLastName("localAdmin")
                .Build();

            this.InternalOrganisation.AddLocalAdministrator(localAdmin);

            this.Transaction.Derive();
            this.Transaction.Commit();

            this.Transaction.SetUser(localAdmin);

            var acl = new DatabaseAccessControl(this.Security, localAdmin)[this.Transaction.GetSingleton().Settings];
            Assert.True(acl.CanRead(M.Settings.CleaningCalculation));
            Assert.False(acl.CanWrite(M.Settings.CleaningCalculation));
        }

        [Fact]
        public void Usergroup()
        {
            var userGroup = new UserGroups(this.Transaction).Administrators;

            var localAdmin = new PersonBuilder(this.Transaction)
                .WithUserName("localadmin")
                .WithLastName("localAdmin")
                .Build();

            this.InternalOrganisation.AddLocalAdministrator(localAdmin);

            this.Transaction.Derive();
            this.Transaction.Commit();

            this.Transaction.SetUser(localAdmin);

            var acl = new DatabaseAccessControl(this.Security, localAdmin)[userGroup];
            Assert.True(acl.CanRead(M.UserGroup.Members));
            Assert.True(acl.CanWrite(M.UserGroup.Members));
        }

        [Fact]
        public void LocalAdminEditPerson()
        {
            var person = this.InternalOrganisation.EmploymentsWhereEmployer.First().Employee;

            var localAdmin = new PersonBuilder(this.Transaction)
                .WithUserName("localadmin")
                .WithLastName("localAdmin")
                .Build();

            this.InternalOrganisation.AddLocalAdministrator(localAdmin);

            this.Transaction.Derive();

            this.Transaction.SetUser(localAdmin);

            var acl = new DatabaseAccessControl(this.Security, localAdmin)[person];
            Assert.False(acl.CanRead(M.Person.UserPasswordHash));
            Assert.False(acl.CanWrite(M.Person.UserPasswordHash));
            Assert.False(acl.CanRead(M.Person.InUserPassword));
            Assert.False(acl.CanWrite(M.Person.InUserPassword));
            Assert.False(acl.CanRead(M.Person.InExistingUserPassword));
            Assert.False(acl.CanWrite(M.Person.InExistingUserPassword));
        }
    }
}
