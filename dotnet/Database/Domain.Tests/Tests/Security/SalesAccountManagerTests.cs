// <copyright file="SalesAccountManagerTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain.Tests
{
    using System.Linq;
    using Allors;
    using Xunit;

    public class SalesAccountManagerSecurityTests : DomainTest, IClassFixture<Fixture>
    {
        public SalesAccountManagerSecurityTests(Fixture fixture) : base(fixture) { }

        public override Config Config => new Config { SetupSecurity = true };

        [Fact]
        public void Person()
        {
            var person = new People(this.Transaction).Extent().First();
            var internalOrganisation = new Organisations(this.Transaction).Extent().First(o => o.IsInternalOrganisation);
            var singleton = this.Transaction.GetSingleton();

            var salesaccm = new PersonBuilder(this.Transaction)
                .WithUserName("salesaccountmanager")
                .WithLastName("salesaccm")
                .Build();

            internalOrganisation.AddLocalSalesAccountManager(salesaccm);

            var userGroups = new UserGroups(this.Transaction);
            userGroups.Creators.AddMember(salesaccm);

            this.Transaction.Derive(true);

            this.Transaction.SetUser(salesaccm);

            var acl = new DatabaseAccessControl(this.Security, salesaccm)[person];
            Assert.True(acl.CanRead(M.Person.FirstName));
            Assert.True(acl.CanWrite(M.Person.FirstName));
            Assert.False(acl.CanRead(M.Person.UserPasswordHash));
            Assert.False(acl.CanWrite(M.Person.UserPasswordHash));
            Assert.False(acl.CanRead(M.Person.InUserPassword));
            Assert.False(acl.CanWrite(M.Person.InUserPassword));
            Assert.False(acl.CanRead(M.Person.InExistingUserPassword));
            Assert.False(acl.CanWrite(M.Person.InExistingUserPassword));
        }

        [Fact]
        public void Organisation()
        {
            var organisation = new Organisations(this.Transaction).Extent().First(o => !o.IsInternalOrganisation);
            var internalOrganisation = new Organisations(this.Transaction).Extent().First(o => o.IsInternalOrganisation);
            var singleton = this.Transaction.GetSingleton();

            var salesaccm = new PersonBuilder(this.Transaction)
                .WithUserName("salesaccountmanager")
                .WithLastName("salesaccm")
                .Build();

            internalOrganisation.AddLocalSalesAccountManager(salesaccm);

            var userGroups = new UserGroups(this.Transaction);
            userGroups.Creators.AddMember(salesaccm);

            this.Transaction.Derive(true);

            this.Transaction.SetUser(salesaccm);

            var acl = new DatabaseAccessControl(this.Security, salesaccm)[organisation];
            Assert.False(acl.CanExecute(M.Organisation.Delete));
        }

        [Fact]
        public void SalesInvoice()
        {
            var salesInvoice = new SalesInvoiceBuilder(this.Transaction).WithBillToCustomer(this.Customer).WithBilledFrom(this.InternalOrganisation).Build();

            this.Transaction.Derive();

            var singleton = this.Transaction.GetSingleton();

            var salesaccm = new PersonBuilder(this.Transaction)
                .WithUserName("salesaccountmanager")
                .WithLastName("salesaccm")
                .Build();

            this.InternalOrganisation.AddLocalSalesAccountManager(salesaccm);

            var userGroups = new UserGroups(this.Transaction);
            userGroups.Creators.AddMember(salesaccm);

            this.Transaction.Derive(true);

            this.Transaction.SetUser(salesaccm);

            Assert.True(salesInvoice.Strategy.IsNewInTransaction);

            var acl = new DatabaseAccessControl(this.Security, salesaccm)[salesInvoice];
            Assert.True(acl.CanRead(M.SalesInvoice.Description));
            Assert.True(acl.CanWrite(M.SalesInvoice.Description));

            this.Transaction.Commit();

            Assert.False(salesInvoice.Strategy.IsNewInTransaction);

            acl = new DatabaseAccessControl(this.Security, salesaccm)[salesInvoice];
            Assert.True(acl.CanRead(M.SalesInvoice.Description));
            Assert.True(acl.CanWrite(M.SalesInvoice.Description));
        }

        [Fact]
        public void SalesInvoiceOtherInternalOrganisation()
        {
            var customer = new OrganisationBuilder(this.Transaction).WithName("Org1").Build();
            var contactMechanism = new PostalAddressBuilder(this.Transaction)
                .WithAddress1("Haverwerf 15")
                .WithLocality("Mechelen")
                .WithCountry(new Countries(this.Transaction).FindBy(M.Country.IsoCode, "BE"))
                .Build();

            var otherInternalOrganisation = new OrganisationBuilder(this.Transaction).WithIsInternalOrganisation(true).WithName("other internalOrganisation").Build();

            new StoreBuilder(this.Transaction)
                .WithName("store")
                .WithBillingProcess(new BillingProcesses(this.Transaction).BillingForShipmentItems)
                .WithInternalOrganisation(otherInternalOrganisation)
                .WithDefaultShipmentMethod(new ShipmentMethods(this.Transaction).Ground)
                .WithDefaultCarrier(new Carriers(this.Transaction).Fedex)
                .WithDefaultCollectionMethod(new PaymentMethods(this.Transaction).Extent().First())
                .WithIsImmediatelyPacked(true)
                .Build();
            this.Transaction.Derive();


            new CustomerRelationshipBuilder(this.Transaction).WithCustomer(customer).WithInternalOrganisation(otherInternalOrganisation).Build();
            this.Transaction.Derive();

            var salesInvoice = new SalesInvoiceBuilder(this.Transaction).WithBillToCustomer(customer).WithAssignedBillToContactMechanism(contactMechanism).WithBilledFrom(otherInternalOrganisation).Build();

            this.Transaction.Derive();

            var singleton = this.Transaction.GetSingleton();

            var salesaccm = new PersonBuilder(this.Transaction)
                .WithUserName("salesaccountmanager")
                .WithLastName("salesaccm")
                .Build();

            this.InternalOrganisation.AddLocalSalesAccountManager(salesaccm);

            this.Transaction.Commit();

            var acl = new DatabaseAccessControl(this.Security, salesaccm)[salesInvoice];
            Assert.False(acl.CanRead(M.SalesInvoice.Description));
            Assert.False(acl.CanWrite(M.SalesInvoice.Description));
        }

        [Fact]
        public void SalesInvoiceItem()
        {
            var customer = new OrganisationBuilder(this.Transaction).WithName("Org1").Build();
            var contactMechanism = new PostalAddressBuilder(this.Transaction)
                .WithAddress1("Haverwerf 15")
                .WithLocality("Mechelen")
                .WithCountry(new Countries(this.Transaction).FindBy(M.Country.IsoCode, "BE"))
                .Build();

            var internalOrganisation = new Organisations(this.Transaction).Extent().First(o => o.IsInternalOrganisation);
            new CustomerRelationshipBuilder(this.Transaction).WithCustomer(customer).WithInternalOrganisation(internalOrganisation).Build();
            this.Transaction.Derive();

            var salesInvoice = new SalesInvoiceBuilder(this.Transaction)
                .WithBilledFrom(internalOrganisation)
                .WithBillToCustomer(customer)
                .WithAssignedBillToContactMechanism(contactMechanism)
                .Build();
            var salesInvoiceItem = new SalesInvoiceItemBuilder(this.Transaction)
                .WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).Service)
                .WithAssignedUnitPrice(1)
                .Build();
            salesInvoice.AddSalesInvoiceItem(salesInvoiceItem);

            this.Transaction.Derive();

            var salesaccm = new PersonBuilder(this.Transaction)
                .WithUserName("salesaccountmanager")
                .WithLastName("salesaccm")
                .Build();

            internalOrganisation.AddLocalSalesAccountManager(salesaccm);

            var userGroups = new UserGroups(this.Transaction);
            userGroups.Creators.AddMember(salesaccm);

            this.Transaction.Derive(true);

            this.Transaction.SetUser(salesaccm);

            Assert.True(salesInvoiceItem.Strategy.IsNewInTransaction);

            var acl = new DatabaseAccessControl(this.Security, salesaccm)[salesInvoiceItem];
            Assert.True(acl.CanRead(M.SalesInvoiceItem.Description));
            Assert.True(acl.CanWrite(M.SalesInvoiceItem.Description));

            this.Transaction.Commit();

            Assert.False(salesInvoiceItem.Strategy.IsNewInTransaction);

            acl = new DatabaseAccessControl(this.Security, salesaccm)[salesInvoiceItem];
            Assert.True(acl.CanRead(M.SalesInvoiceItem.Description));
            Assert.True(acl.CanWrite(M.SalesInvoiceItem.Description));
        }

        [Fact]
        public void SalesOrderItem()
        {
            var customer = new OrganisationBuilder(this.Transaction).WithName("Org1").Build();
            var contactMechanism = new PostalAddressBuilder(this.Transaction)
                .WithAddress1("Haverwerf 15")
                .WithLocality("Mechelen")
                .WithCountry(new Countries(this.Transaction).FindBy(M.Country.IsoCode, "BE"))
                .Build();

            var internalOrganisation = new Organisations(this.Transaction).Extent().First(o => o.IsInternalOrganisation);
            new CustomerRelationshipBuilder(this.Transaction).WithCustomer(customer).WithInternalOrganisation(internalOrganisation).Build();
            this.Transaction.Derive();

            var salesaccm = new PersonBuilder(this.Transaction)
                .WithUserName("salesaccountmanager")
                .WithLastName("salesaccm")
                .Build();

            internalOrganisation.AddLocalSalesAccountManager(salesaccm);

            var userGroups = new UserGroups(this.Transaction);
            userGroups.Creators.AddMember(salesaccm);

            this.Transaction.Derive(true);

            this.Transaction.SetUser(salesaccm);

            var salesOrder = new SalesOrderBuilder(this.Transaction).WithTakenBy(internalOrganisation).WithShipToCustomer(customer).WithAssignedShipToAddress(contactMechanism).Build();
            var salesOrderItem = new SalesOrderItemBuilder(this.Transaction).WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).Service).WithAssignedUnitPrice(1).Build();
            salesOrder.AddSalesOrderItem(salesOrderItem);

            this.Transaction.Derive(true);

            Assert.True(salesOrderItem.Strategy.IsNewInTransaction);

            var acl = new DatabaseAccessControl(this.Security, salesaccm)[salesOrderItem];
            Assert.True(acl.CanRead(M.SalesOrderItem.Description));
            Assert.True(acl.CanWrite(M.SalesOrderItem.Description));
            Assert.False(acl.CanRead(M.SalesOrderItem.CostOfGoodsSold));
            Assert.False(acl.CanWrite(M.SalesOrderItem.CostOfGoodsSold));

            this.Transaction.Commit();

            Assert.False(salesOrderItem.Strategy.IsNewInTransaction);

            acl = new DatabaseAccessControl(this.Security, salesaccm)[salesOrderItem];
            Assert.True(acl.CanRead(M.SalesOrderItem.Description));
            Assert.True(acl.CanWrite(M.SalesOrderItem.Description));
            Assert.False(acl.CanRead(M.SalesOrderItem.CostOfGoodsSold));
            Assert.False(acl.CanWrite(M.SalesOrderItem.CostOfGoodsSold));
        }

        [Fact]
        public void OwnInternalOrganisation()
        {
            var supplier = new OrganisationBuilder(this.Transaction).WithName("supplier").Build();
            var internalOrganisation = new Organisations(this.Transaction).Extent().First(o => o.IsInternalOrganisation);

            new SupplierRelationshipBuilder(this.Transaction)
                .WithSupplier(supplier)
                .WithInternalOrganisation(internalOrganisation)
                .WithFromDate(Transaction.Now())
                .Build();

            this.Transaction.Derive();

            var singleton = this.Transaction.GetSingleton();

            var salesaccm = new PersonBuilder(this.Transaction)
                .WithUserName("salesaccountmanager")
                .WithLastName("salesaccm")
                .Build();

            internalOrganisation.AddLocalSalesAccountManager(salesaccm);

            var userGroups = new UserGroups(this.Transaction);
            userGroups.Creators.AddMember(salesaccm);

            this.Transaction.Derive(true);

            this.Transaction.SetUser(salesaccm);

            var acl = new DatabaseAccessControl(this.Security, salesaccm)[internalOrganisation];
            Assert.True(acl.CanExecute(M.Organisation.CreatePurchaseOrder));
        }

        [Fact]
        public void PurchaseOrder()
        {
            var supplier = new OrganisationBuilder(this.Transaction).WithName("supplier").Build();
            var internalOrganisation = new Organisations(this.Transaction).Extent().First(o => o.IsInternalOrganisation);

            new SupplierRelationshipBuilder(this.Transaction)
                .WithSupplier(supplier)
                .WithInternalOrganisation(internalOrganisation)
                .WithFromDate(Transaction.Now())
                .Build();

            this.Transaction.Derive();

            var singleton = this.Transaction.GetSingleton();

            var salesaccm = new PersonBuilder(this.Transaction)
                .WithUserName("salesaccountmanager")
                .WithLastName("salesaccm")
                .Build();

            internalOrganisation.AddLocalSalesAccountManager(salesaccm);

            var userGroups = new UserGroups(this.Transaction);
            userGroups.Creators.AddMember(salesaccm);

            this.Transaction.Derive(true);

            this.Transaction.SetUser(salesaccm);

            var purchaseOrder = new PurchaseOrderBuilder(this.Transaction).WithTakenViaSupplier(supplier).WithOrderedBy(internalOrganisation).Build();

            this.Transaction.Derive();

            Assert.True(purchaseOrder.Strategy.IsNewInTransaction);

            var acl = new DatabaseAccessControl(this.Security, salesaccm)[purchaseOrder];
            Assert.True(acl.CanRead(M.PurchaseOrder.Description));
            Assert.True(acl.CanWrite(M.PurchaseOrder.Description));

            this.Transaction.Commit();

            Assert.False(purchaseOrder.Strategy.IsNewInTransaction);

            acl = new DatabaseAccessControl(this.Security, salesaccm)[purchaseOrder];
            Assert.True(acl.CanRead(M.PurchaseOrder.Description));
            Assert.True(acl.CanWrite(M.PurchaseOrder.Description));
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

            var salesaccm = new PersonBuilder(this.Transaction)
                .WithUserName("salesaccountmanager")
                .WithLastName("salesaccm")
                .Build();

            this.InternalOrganisation.AddLocalSalesAccountManager(salesaccm);
            this.Transaction.Derive();

            this.Transaction.SetUser(salesaccm);

            var purchaseOrder = new PurchaseOrderBuilder(this.Transaction)
                .WithOrderedBy(otherInternalOrganisation)
                .WithAssignedBillToContactMechanism(contactMechanism)
                .WithTakenViaSupplier(supplier)
                .Build();

            this.Transaction.Derive();

            Assert.True(purchaseOrder.ExistSecurityTokens);

            var acl = new DatabaseAccessControl(this.Security, salesaccm)[purchaseOrder];
            Assert.False(acl.CanRead(M.PurchaseOrder.Description));
            Assert.False(acl.CanWrite(M.PurchaseOrder.Description));
        }

        [Fact]
        public void PurchaseOrderItem()
        {
            var supplier = new OrganisationBuilder(this.Transaction).WithName("supplier").Build();
            var internalOrganisation = new Organisations(this.Transaction).Extent().First(o => o.IsInternalOrganisation);

            new SupplierRelationshipBuilder(this.Transaction)
                .WithSupplier(supplier)
                .WithInternalOrganisation(internalOrganisation)
                .WithFromDate(Transaction.Now())
                .Build();

            this.Transaction.Derive();

            var singleton = this.Transaction.GetSingleton();

            var salesaccm = new PersonBuilder(this.Transaction)
                .WithUserName("salesaccountmanager")
                .WithLastName("salesaccm")
                .Build();

            internalOrganisation.AddLocalSalesAccountManager(salesaccm);

            var userGroups = new UserGroups(this.Transaction);
            userGroups.Creators.AddMember(salesaccm);

            this.Transaction.Derive(true);

            this.Transaction.SetUser(salesaccm);

            var purchaseOrder = new PurchaseOrderBuilder(this.Transaction).WithTakenViaSupplier(supplier).WithOrderedBy(internalOrganisation).Build();
            var purchaseOrderItem = new PurchaseOrderItemBuilder(this.Transaction)
                .WithDescription("something")
                .WithQuantityOrdered(1)
                .WithAssignedUnitPrice(1)
                .WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).Service)
                .Build();
            purchaseOrder.AddPurchaseOrderItem(purchaseOrderItem);

            this.Transaction.Derive();

            Assert.True(purchaseOrderItem.Strategy.IsNewInTransaction);

            var acl = new DatabaseAccessControl(this.Security, salesaccm)[purchaseOrderItem];
            Assert.True(acl.CanRead(M.PurchaseOrderItem.Description));
            Assert.True(acl.CanWrite(M.PurchaseOrderItem.Description));

            this.Transaction.Commit();

            Assert.False(purchaseOrderItem.Strategy.IsNewInTransaction);

            acl = new DatabaseAccessControl(this.Security, salesaccm)[purchaseOrderItem];
            Assert.True(acl.CanRead(M.PurchaseOrderItem.Description));
            Assert.True(acl.CanWrite(M.PurchaseOrderItem.Description));
        }

        [Fact]
        public void Good()
        {
            var good = new UnifiedGoodBuilder(this.Transaction)
                .WithName("Unified Good")
                .WithVatRegime(new VatRegimes(this.Transaction).ZeroRated)
                .Build();
            var internalOrganisation = new Organisations(this.Transaction).Extent().First(o => o.IsInternalOrganisation);
            var singleton = this.Transaction.GetSingleton();

            var salesaccm = new PersonBuilder(this.Transaction)
                .WithUserName("salesaccountmanager")
                .WithLastName("salesaccm")
                .Build();

            internalOrganisation.AddLocalSalesAccountManager(salesaccm);

            var userGroups = new UserGroups(this.Transaction);
            userGroups.Creators.AddMember(salesaccm);

            this.Transaction.Derive();
            this.Transaction.Commit();

            this.Transaction.SetUser(salesaccm);

            var acl = new DatabaseAccessControl(this.Security, salesaccm)[good];
            Assert.True(acl.CanRead(M.Good.Name));
            Assert.False(acl.CanWrite(M.Good.Name));
        }

        [Fact]
        public void WorkEffortInventoryAssignment()
        {
            var internalOrganisation = new Organisations(this.Transaction).Extent().First(o => o.IsInternalOrganisation);
            var customer = new OrganisationBuilder(this.Transaction).WithName("Org1").Build();
            new CustomerRelationshipBuilder(this.Transaction).WithCustomer(customer).WithInternalOrganisation(internalOrganisation).Build();

            var salesaccm = new PersonBuilder(this.Transaction)
                .WithUserName("salesaccountmanager")
                .WithLastName("salesaccm")
                .Build();

            internalOrganisation.AddLocalSalesAccountManager(salesaccm);

            var userGroups = new UserGroups(this.Transaction);
            userGroups.Creators.AddMember(salesaccm);

            this.Transaction.Derive();
            this.Transaction.Commit();

            var workTask = new WorkTaskBuilder(this.Transaction)
                .WithName("Activity")
                .WithCustomer(customer)
                .WithTakenBy(internalOrganisation)
                .Build();

            this.Transaction.Derive();

            var part = new NonUnifiedPartBuilder(this.Transaction)
                .WithProductIdentification(new PartNumberBuilder(this.Transaction)
                    .WithIdentification("P1")
                    .WithProductIdentificationType(new ProductIdentificationTypes(this.Transaction).Part).Build())
                .Build();

            this.Transaction.Derive(true);

            new InventoryItemTransactionBuilder(this.Transaction).WithQuantity(100).WithReason(new InventoryTransactionReasons(this.Transaction).Unknown).WithPart(part).Build();
            this.Transaction.Derive(true);

            var inventoryAssignment = new WorkEffortInventoryAssignmentBuilder(this.Transaction)
                .WithAssignment(workTask)
                .WithInventoryItem(part.InventoryItemsWherePart.First())
                .WithQuantity(10)
                .Build();

            this.Transaction.Derive(true);
            this.Transaction.Commit();

            this.Transaction.SetUser(salesaccm);

            var acl = new DatabaseAccessControl(this.Security, salesaccm)[inventoryAssignment];
            Assert.True(acl.CanRead(M.WorkEffortInventoryAssignment.AssignedUnitSellingPrice));
            Assert.True(acl.CanWrite(M.WorkEffortInventoryAssignment.AssignedUnitSellingPrice));
            Assert.True(acl.CanRead(M.WorkEffortInventoryAssignment.AssignedBillableQuantity));
            Assert.True(acl.CanWrite(M.WorkEffortInventoryAssignment.AssignedBillableQuantity));
        }
    }
}
