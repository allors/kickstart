// <copyright file="EmployeeTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain.Tests
{
    using System.Linq;
    using Allors;
    using Allors.Database.Domain.TestPopulation;
    using Xunit;


    public class LocalEmployeeSecurityTests : DomainTest, IClassFixture<Fixture>
    {
        public LocalEmployeeSecurityTests(Fixture fixture) : base(fixture) { }

        public override Config Config => new Config { SetupSecurity = true };

        [Fact]
        public void OwnInternalOrganisation()
        {
            this.Transaction.SetUser(this.Employee);

            var acl = new DatabaseAccessControl(this.Security, this.Employee)[this.InternalOrganisation];
            Assert.True(acl.CanRead(M.Organisation.Name));
            Assert.False(acl.CanWrite(M.Organisation.Name));
            Assert.False(acl.CanExecute(M.Organisation.Delete));
        }

        [Fact]
        public void OtherInternalOrganisation()
        {
            var organisation = new OrganisationBuilder(this.Transaction).WithIsInternalOrganisation(true).WithName("other internalOrganisation").Build();
            this.Transaction.Derive();

            this.Transaction.SetUser(this.Employee);

            var acl = new DatabaseAccessControl(this.Security, this.Employee)[organisation];
            Assert.True(acl.CanRead(M.Organisation.Name));
            Assert.False(acl.CanWrite(M.Organisation.Name));
            Assert.False(acl.CanExecute(M.Organisation.Delete));
        }

        [Fact]
        public void OrganisationIsCustomerAtOwnInternalOrganisation()
        {
            var organisation = this.Customer;
            var employee = this.Employee;

            this.Transaction.SetUser(employee);

            var acl = new DatabaseAccessControl(this.Security, employee)[organisation];
            Assert.True(acl.CanRead(M.Organisation.Name));
            Assert.False(acl.CanWrite(M.Organisation.Name));
            Assert.False(acl.CanExecute(M.Organisation.Delete));
        }

        [Fact]
        public void OrganisationIsCustomerAtOtherInternalOrganisation()
        {
            var otherInternalOrganisation = new OrganisationBuilder(this.Transaction).WithIsInternalOrganisation(true).WithName("other internalOrganisation").Build();
            var organisation = new OrganisationBuilder(this.Transaction).WithName("Org1").Build();
            new CustomerRelationshipBuilder(this.Transaction).WithCustomer(organisation).WithInternalOrganisation(otherInternalOrganisation).Build();

            this.Transaction.Derive(true);

            this.Transaction.SetUser(this.Employee);

            var acl = new DatabaseAccessControl(this.Security, this.Employee)[organisation];
            Assert.True(acl.CanRead(M.Organisation.Name));
            Assert.False(acl.CanWrite(M.Organisation.Name));
            Assert.False(acl.CanExecute(M.Organisation.Delete));
        }

        [Fact]
        public void OrganisationIsSupplierAtOwnInternalOrganisation()
        {
            this.Transaction.SetUser(this.Employee);

            var acl = new DatabaseAccessControl(this.Security, this.Employee)[this.Supplier];
            Assert.True(acl.CanRead(M.Organisation.Name));
            Assert.False(acl.CanWrite(M.Organisation.Name));
            Assert.False(acl.CanExecute(M.Organisation.Delete));
        }

        [Fact]
        public void OrganisationIsSupplierAtOtherInternalOrganisation()
        {
            var otherInternalOrganisation = new OrganisationBuilder(this.Transaction).WithIsInternalOrganisation(true).WithName("other internalOrganisation").Build();
            var organisation = new OrganisationBuilder(this.Transaction).WithName("Org1").Build();
            new SupplierRelationshipBuilder(this.Transaction).WithSupplier(organisation).WithInternalOrganisation(otherInternalOrganisation).Build();

            this.Transaction.Derive(true);

            this.Transaction.SetUser(this.Employee);

            var acl = new DatabaseAccessControl(this.Security, this.Employee)[organisation];
            Assert.True(acl.CanRead(M.Organisation.Name));
            Assert.False(acl.CanWrite(M.Organisation.Name));
            Assert.False(acl.CanExecute(M.Organisation.Delete));
        }

        [Fact]
        public void OrganisationIsSubcontractorAtOwnInternalOrganisation()
        {
            this.Transaction.SetUser(this.Employee);

            var acl = new DatabaseAccessControl(this.Security, this.Employee)[this.SubContractor];
            Assert.True(acl.CanRead(M.Organisation.Name));
            Assert.False(acl.CanWrite(M.Organisation.Name));
            Assert.False(acl.CanExecute(M.Organisation.Delete));
        }

        [Fact]
        public void OrganisationIsSubcontractorAtOtherInternalOrganisation()
        {
            var otherInternalOrganisation = new OrganisationBuilder(this.Transaction).WithIsInternalOrganisation(true).WithName("other internalOrganisation").Build();
            var organisation = new OrganisationBuilder(this.Transaction).WithName("Org1").Build();
            new SubContractorRelationshipBuilder(this.Transaction).WithSubContractor(organisation).WithContractor(otherInternalOrganisation).Build();

            this.Transaction.Derive(true);

            this.Transaction.SetUser(this.Employee);

            var acl = new DatabaseAccessControl(this.Security, this.Employee)[organisation];
            Assert.True(acl.CanRead(M.Organisation.Name));
            Assert.False(acl.CanWrite(M.Organisation.Name));
            Assert.False(acl.CanExecute(M.Organisation.Delete));
        }

        [Fact]
        public void SalesInvoiceOwnInternalOrganisation()
        {
            var salesInvoice = new SalesInvoiceBuilder(this.Transaction).WithBillToCustomer(this.Customer).WithBilledFrom(this.InternalOrganisation).Build();

            this.Transaction.Derive();

            this.Transaction.SetUser(this.Employee);

            Assert.True(salesInvoice.Strategy.IsNewInTransaction);

            var acl = new DatabaseAccessControl(this.Security, this.Employee)[salesInvoice];
            Assert.True(acl.CanRead(M.SalesInvoice.Description));
            Assert.False(acl.CanWrite(M.SalesInvoice.Description));
            Assert.False(acl.CanExecute(M.SalesInvoice.Print));

            this.Transaction.Commit();

            Assert.False(salesInvoice.Strategy.IsNewInTransaction);

            Assert.True(salesInvoice.ExistSecurityTokens);

            acl = new DatabaseAccessControl(this.Security, this.Employee)[salesInvoice];
            Assert.True(acl.CanRead(M.SalesInvoice.Description));
            Assert.False(acl.CanWrite(M.SalesInvoice.Description));
            Assert.False(acl.CanExecute(M.SalesInvoice.Print));
        }

        [Fact]
        public void SalesInvoiceOtherInternalOrganisation()
        {
            var netherlands = new Countries(this.Transaction).CountryByIsoCode["NL"];
            var euro = netherlands.Currency;

            var bank = new BankBuilder(this.Transaction).WithCountry(netherlands).WithName("RABOBANK GROEP").WithBic("RABONL2U").Build();

            var ownBankAccount = new OwnBankAccountBuilder(this.Transaction)
                .WithDescription("BE23 3300 6167 6391")
                .WithBankAccount(new BankAccountBuilder(this.Transaction).WithBank(bank).WithCurrency(euro).WithIban("NL50RABO0109546784").WithNameOnAccount("AccountName").Build())
                .Build();

            var customer = new OrganisationBuilder(this.Transaction).WithName("Org1").Build();
            var contactMechanism = new PostalAddressBuilder(this.Transaction)
                .WithAddress1("Haverwerf 15")
                .WithLocality("Mechelen")
                .WithCountry(new Countries(this.Transaction).FindBy(M.Country.IsoCode, "BE"))
                .Build();

            var otherInternalOrganisation = new OrganisationBuilder(this.Transaction).WithIsInternalOrganisation(true).WithName("other internalOrganisation").Build();
            this.Transaction.Derive(true);

            new CustomerRelationshipBuilder(this.Transaction).WithCustomer(customer).WithInternalOrganisation(otherInternalOrganisation).Build();

            var salesInvoice = new SalesInvoiceBuilder(this.Transaction)
                .WithBillToCustomer(customer)
                .WithAssignedBillToContactMechanism(contactMechanism)
                .WithBilledFrom(otherInternalOrganisation)
                .WithInvoiceNumber("1")
                .WithStore(new StoreBuilder(this.Transaction)
                    .WithName("store")
                    .WithInternalOrganisation(otherInternalOrganisation)
                    .WithDefaultCarrier(new Carriers(this.Transaction).Fedex)
                    .WithDefaultShipmentMethod(new ShipmentMethods(this.Transaction).Ground)
                    .WithDefaultCollectionMethod(ownBankAccount)
                    .Build())
                .Build();

            this.Transaction.Derive();

            this.Transaction.SetUser(this.Employee);

            Assert.True(salesInvoice.Strategy.IsNewInTransaction);

            Assert.True(salesInvoice.ExistSecurityTokens);

            var acl = new DatabaseAccessControl(this.Security, this.Employee)[salesInvoice];
            Assert.False(acl.CanRead(M.SalesInvoice.Description));
            Assert.False(acl.CanWrite(M.SalesInvoice.Description));
            Assert.False(acl.CanExecute(M.SalesInvoice.Print));

            this.Transaction.Commit();

            Assert.False(salesInvoice.Strategy.IsNewInTransaction);

            acl = new DatabaseAccessControl(this.Security, this.Employee)[salesInvoice];
            Assert.False(acl.CanRead(M.SalesInvoice.Description));
            Assert.False(acl.CanWrite(M.SalesInvoice.Description));
            Assert.False(acl.CanExecute(M.SalesInvoice.Print));
        }

        [Fact]
        public void SalesOrderOwnInternalOrganisation()
        {
            var salesOrder = new SalesOrderBuilder(this.Transaction).WithTakenBy(this.InternalOrganisation).WithBillToCustomer(this.Customer).Build();

            this.Transaction.Derive();

            this.Transaction.SetUser(this.Employee);

            Assert.True(salesOrder.Strategy.IsNewInTransaction);

            var acl = new DatabaseAccessControl(this.Security, this.Employee)[salesOrder];
            Assert.True(acl.CanRead(M.SalesOrder.Description));
            Assert.False(acl.CanWrite(M.SalesOrder.Description));

            this.Transaction.Commit();

            Assert.False(salesOrder.Strategy.IsNewInTransaction);

            Assert.True(salesOrder.ExistSecurityTokens);

            acl = new DatabaseAccessControl(this.Security, this.Employee)[salesOrder];
            Assert.True(acl.CanRead(M.SalesOrder.Description));
            Assert.False(acl.CanWrite(M.SalesOrder.Description));

            var salesOrderItem = new SalesOrderItemBuilder(this.Transaction).WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).Other).WithAssignedUnitPrice(1).Build();
            salesOrder.AddSalesOrderItem(salesOrderItem);

            this.Transaction.Derive();

            acl = new DatabaseAccessControl(this.Security, this.Employee)[salesOrderItem];
            Assert.True(acl.CanRead(M.SalesOrderItem.Description));
            Assert.False(acl.CanWrite(M.SalesOrderItem.Description));
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

            var salesOrder = new SalesOrderBuilder(this.Transaction)
                .WithTakenBy(otherInternalOrganisation)
                .WithBillToCustomer(customer)
                .WithAssignedTakenByContactMechanism(contactMechanism)
                .WithAssignedBillToContactMechanism(contactMechanism)
                .Build();

            this.Transaction.Derive();

            this.Transaction.SetUser(this.Employee);

            Assert.True(salesOrder.ExistSecurityTokens);

            var acl = new DatabaseAccessControl(this.Security, this.Employee)[salesOrder];
            Assert.False(acl.CanRead(M.SalesOrder.Description));
            Assert.False(acl.CanWrite(M.SalesOrder.Description));
        }

        [Fact]
        public void CustomerShipmentOwnInternalOrganisation()
        {
            var shipment = new CustomerShipmentBuilder(this.Transaction).WithShipFromParty(this.InternalOrganisation).WithShipToParty(this.Customer).Build();

            this.Transaction.Derive();

            this.Transaction.SetUser(this.Employee);

            Assert.True(shipment.ExistSecurityTokens);

            var acl = new DatabaseAccessControl(this.Security, this.Employee)[shipment];
            Assert.True(acl.CanRead(M.CustomerShipment.Comment));
            Assert.False(acl.CanWrite(M.CustomerShipment.Comment));
            Assert.False(acl.CanExecute(M.CustomerShipment.Cancel));
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

            this.Transaction.SetUser(this.Employee);

            Assert.True(shipment.ExistSecurityTokens);

            var acl = new DatabaseAccessControl(this.Security, this.Employee)[shipment];
            Assert.False(acl.CanRead(M.CustomerShipment.Comment));
            Assert.False(acl.CanWrite(M.CustomerShipment.Comment));
            Assert.False(acl.CanExecute(M.CustomerShipment.Cancel));
        }

        [Fact]
        public void PurchaseInvoiceOwnInternalOrganisation()
        {
            var purchaseInvoice = new PurchaseInvoiceBuilder(this.Transaction).WithBilledTo(this.InternalOrganisation).WithBilledFrom(this.Supplier).WithPurchaseInvoiceType(new PurchaseInvoiceTypes(this.Transaction).PurchaseInvoice).Build();

            this.Transaction.Derive();

            this.Transaction.SetUser(this.Employee);

            Assert.True(purchaseInvoice.Strategy.IsNewInTransaction);

            var acl = new DatabaseAccessControl(this.Security, this.Employee)[purchaseInvoice];
            Assert.True(acl.CanRead(M.PurchaseInvoice.Description));
            Assert.False(acl.CanWrite(M.PurchaseInvoice.Description));
            Assert.False(acl.CanExecute(M.PurchaseInvoice.Print));

            this.Transaction.Commit();

            Assert.False(purchaseInvoice.Strategy.IsNewInTransaction);

            Assert.True(purchaseInvoice.ExistSecurityTokens);

            acl = new DatabaseAccessControl(this.Security, this.Employee)[purchaseInvoice];
            Assert.True(acl.CanRead(M.PurchaseInvoice.Description));
            Assert.False(acl.CanWrite(M.PurchaseInvoice.Description));
            Assert.False(acl.CanExecute(M.PurchaseInvoice.Print));
        }

        [Fact]
        public void PurchaseInvoiceOtherInternalOrganisation()
        {
            var otherInternalOrganisation = new OrganisationBuilder(this.Transaction).WithIsInternalOrganisation(true).WithName("other internalOrganisation").Build();

            var supplier = new OrganisationBuilder(this.Transaction).WithName("other organisation").Build();
            new SupplierRelationshipBuilder(this.Transaction).WithInternalOrganisation(otherInternalOrganisation).WithSupplier(supplier).Build();

            var purchaseInvoice = new PurchaseInvoiceBuilder(this.Transaction).WithBilledTo(otherInternalOrganisation).WithBilledFrom(supplier).WithPurchaseInvoiceType(new PurchaseInvoiceTypes(this.Transaction).PurchaseInvoice).Build();

            this.Transaction.Derive();

            this.Transaction.SetUser(this.Employee);

            Assert.True(purchaseInvoice.Strategy.IsNewInTransaction);

            var acl = new DatabaseAccessControl(this.Security, this.Employee)[purchaseInvoice];
            Assert.False(acl.CanRead(M.PurchaseInvoice.Description));
            Assert.False(acl.CanWrite(M.PurchaseInvoice.Description));
            Assert.False(acl.CanExecute(M.PurchaseInvoice.Print));

            this.Transaction.Commit();

            Assert.False(purchaseInvoice.Strategy.IsNewInTransaction);

            Assert.True(purchaseInvoice.ExistSecurityTokens);

            acl = new DatabaseAccessControl(this.Security, this.Employee)[purchaseInvoice];
            Assert.False(acl.CanRead(M.PurchaseInvoice.Description));
            Assert.False(acl.CanWrite(M.PurchaseInvoice.Description));
            Assert.False(acl.CanExecute(M.PurchaseInvoice.Print));
        }

        [Fact]
        public void RequestForQuoteOwnInternalOrganisation()
        {
            var request = new RequestForQuoteBuilder(this.Transaction).WithRecipient(this.InternalOrganisation).WithOriginator(this.Customer).Build();

            this.Transaction.Derive();

            this.Transaction.SetUser(this.Employee);

            Assert.True(request.ExistSecurityTokens);

            var acl = new DatabaseAccessControl(this.Security, this.Employee)[request];
            Assert.True(acl.CanRead(M.RequestForQuote.Comment));
            Assert.False(acl.CanWrite(M.RequestForQuote.Comment));
            Assert.False(acl.CanExecute(M.RequestForQuote.Cancel));
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

            var shipment = new RequestForQuoteBuilder(this.Transaction).WithRecipient(otherInternalOrganisation).WithOriginator(organisation).Build();

            this.Transaction.Derive();

            this.Transaction.SetUser(this.Employee);

            Assert.True(shipment.ExistSecurityTokens);

            var acl = new DatabaseAccessControl(this.Security, this.Employee)[shipment];
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

            this.Transaction.SetUser(this.Employee);

            Assert.True(quote.ExistSecurityTokens);

            var acl = new DatabaseAccessControl(this.Security, this.Employee)[quote];
            Assert.True(acl.CanRead(M.ProductQuote.Comment));
            Assert.False(acl.CanWrite(M.ProductQuote.Comment));
            Assert.False(acl.CanExecute(M.ProductQuote.Cancel));
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

            this.Transaction.SetUser(this.Employee);

            Assert.True(quote.ExistSecurityTokens);

            var acl = new DatabaseAccessControl(this.Security, this.Employee)[quote];
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

            this.Transaction.SetUser(this.Employee);

            Assert.True(inventoryItemTransaction.ExistSecurityTokens);

            var acl = new DatabaseAccessControl(this.Security, this.Employee)[inventoryItemTransaction];
            Assert.True(acl.CanRead(M.InventoryItemTransaction.Quantity));
            Assert.False(acl.CanWrite(M.InventoryItemTransaction.Quantity));
            Assert.False(acl.CanRead(M.InventoryItemTransaction.Cost));
        }

        [Fact]
        public void InventoryItemTransactionOtherInventory()
        {
            var otherInternalOrganisation = new OrganisationBuilder(this.Transaction).WithIsInternalOrganisation(true).WithName("other internalOrganisation").WithQuoteSequence(new QuoteSequences(this.Transaction).EnforcedSequence).Build();
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

            this.Transaction.SetUser(this.Employee);

            Assert.True(inventoryItemTransaction.ExistSecurityTokens);

            var acl = new DatabaseAccessControl(this.Security, this.Employee)[inventoryItemTransaction];
            Assert.False(acl.CanRead(M.InventoryItemTransaction.Quantity));
            Assert.False(acl.CanWrite(M.InventoryItemTransaction.Quantity));
        }

        [Fact]
        public void WorkTaskOwnInternalOrganisation()
        {
            var workTask = new WorkTaskBuilder(this.Transaction).WithName("Activity").WithCustomer(this.Customer).WithTakenBy(this.InternalOrganisation).Build();

            this.Transaction.Derive();

            this.Transaction.SetUser(this.Employee);

            Assert.True(workTask.ExistSecurityTokens);

            var acl = new DatabaseAccessControl(this.Security, this.Employee)[workTask];
            Assert.True(acl.CanRead(M.WorkTask.WorkDone));
            Assert.False(acl.CanWrite(M.WorkTask.WorkDone));
        }

        [Fact]
        public void WorkTaskOtherInternalOrganisation()
        {
            var otherInternalOrganisation = new OrganisationBuilder(this.Transaction).WithIsInternalOrganisation(true).WithName("other internalOrganisation").Build();
            this.Transaction.Derive();

            var workTask = new WorkTaskBuilder(this.Transaction).WithName("Activity").WithCustomer(this.Customer).WithTakenBy(otherInternalOrganisation).Build();

            this.Transaction.Derive();

            this.Transaction.SetUser(this.Employee);

            Assert.True(workTask.ExistSecurityTokens);

            var acl = new DatabaseAccessControl(this.Security, this.Employee)[workTask];
            Assert.False(acl.CanRead(M.WorkTask.WorkDone));
            Assert.False(acl.CanWrite(M.WorkTask.WorkDone));
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

            this.Transaction.SetUser(this.Employee);

            Assert.True(workRequirement.ExistSecurityTokens);

            var acl = new DatabaseAccessControl(this.Security, this.Employee)[workRequirement];
            Assert.True(acl.CanRead(M.WorkRequirement.Description));
            Assert.False(acl.CanWrite(M.WorkRequirement.Description));
            Assert.False(acl.CanExecute(M.WorkRequirement.Cancel));
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

            this.Transaction.SetUser(this.Employee);

            Assert.True(workRequirement.ExistSecurityTokens);

            var acl = new DatabaseAccessControl(this.Security, this.Employee)[workRequirement];
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

            this.Transaction.SetUser(this.Employee);

            Assert.True(fufillment.ExistSecurityTokens);

            var acl = new DatabaseAccessControl(this.Security, this.Employee)[fufillment];
            Assert.True(acl.CanRead(M.WorkRequirementFulfillment.FullfilledBy));
            Assert.False(acl.CanWrite(M.WorkRequirementFulfillment.FullfilledBy));
            Assert.False(acl.CanExecute(M.WorkRequirementFulfillment.Delete));
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

            this.Transaction.SetUser(this.Employee);

            Assert.True(fufillment.ExistSecurityTokens);

            var acl = new DatabaseAccessControl(this.Security, this.Employee)[fufillment];
            Assert.False(acl.CanRead(M.WorkRequirementFulfillment.FullfilledBy));
            Assert.False(acl.CanWrite(M.WorkRequirementFulfillment.FullfilledBy));
            Assert.False(acl.CanExecute(M.WorkRequirementFulfillment.Delete));
        }

        [Fact]
        public void WorkEffortInventoryAssignmentOwnInternalOrganisation()
        {
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

            this.Transaction.SetUser(this.Employee);

            Assert.True(inventoryAssignment.ExistSecurityTokens);

            var acl = new DatabaseAccessControl(this.Security, this.Employee)[inventoryAssignment];
            Assert.True(acl.CanRead(M.WorkEffortInventoryAssignment.InventoryItem));
            Assert.False(acl.CanWrite(M.WorkEffortInventoryAssignment.InventoryItem));
            Assert.True(acl.CanRead(M.WorkEffortInventoryAssignment.Quantity));
            Assert.False(acl.CanWrite(M.WorkEffortInventoryAssignment.Quantity));
            Assert.False(acl.CanRead(M.WorkEffortInventoryAssignment.AssignedBillableQuantity));
            Assert.False(acl.CanWrite(M.WorkEffortInventoryAssignment.AssignedBillableQuantity));
            Assert.False(acl.CanRead(M.WorkEffortInventoryAssignment.UnitSellingPrice));
            Assert.False(acl.CanRead(M.WorkEffortInventoryAssignment.AssignedUnitSellingPrice));
            Assert.False(acl.CanWrite(M.WorkEffortInventoryAssignment.AssignedUnitSellingPrice));
            Assert.False(acl.CanRead(M.WorkEffortInventoryAssignment.UnitPurchasePrice));
            Assert.False(acl.CanRead(M.WorkEffortInventoryAssignment.CostOfGoodsSold));
        }

        [Fact]
        public void WorkEffortInventoryAssignmentOtherInternalOrganisation()
        {
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

            var inventoryAssignment = new WorkEffortInventoryAssignmentBuilder(this.Transaction)
                .WithAssignment(workTask)
                .WithInventoryItem(part.InventoryItemsWherePart.First())
                .WithQuantity(10)
                .Build();

            this.Transaction.Derive();

            Assert.Equal(new WorkEffortStates(this.Transaction).Created, workTask.WorkEffortState);

            this.Transaction.SetUser(this.Employee);

            Assert.True(inventoryAssignment.ExistSecurityTokens);

            var acl = new DatabaseAccessControl(this.Security, this.Employee)[inventoryAssignment];
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
        public void SupplierOfferingOwnInternalOrganisation()
        {
            var part = new NonUnifiedPartBuilder(this.Transaction).WithName("name").Build();
            var offering = new SupplierOfferingBuilder(this.Transaction).WithPart(part).WithSupplier(this.Supplier).WithPrice(1).WithUnitOfMeasure(new UnitsOfMeasure(this.Transaction).Piece).Build();

            this.Transaction.Derive();

            this.Transaction.SetUser(this.Employee);

            Assert.True(offering.ExistSecurityTokens);

            var acl = new DatabaseAccessControl(this.Security, this.Employee)[offering];
            Assert.True(acl.CanRead(M.SupplierOffering.SupplierProductId));
            Assert.False(acl.CanWrite(M.SupplierOffering.SupplierProductId));
            Assert.False(acl.CanRead(M.SupplierOffering.Price));
            Assert.False(acl.CanWrite(M.SupplierOffering.Price));
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

            this.Transaction.SetUser(this.Employee);

            Assert.True(offering.ExistSecurityTokens);

            var acl = new DatabaseAccessControl(this.Security, this.Employee)[offering];
            Assert.False(acl.CanRead(M.SupplierOffering.Price));
            Assert.False(acl.CanWrite(M.SupplierOffering.Price));
        }

        [Fact]
        public void PersonIsContactForOwnCustomerOrganisation()
        {
            this.Transaction.SetUser(this.Employee);

            var contact = this.Customer.OrganisationContactRelationshipsWhereOrganisation.First().Contact;

            Assert.True(contact.ExistSecurityTokens);

            var acl = new DatabaseAccessControl(this.Security, this.Employee)[contact];
            Assert.True(acl.CanRead(M.Person.FirstName));
            Assert.False(acl.CanWrite(M.Person.FirstName));
            Assert.False(acl.CanRead(M.Person.UserPasswordHash));
            Assert.False(acl.CanWrite(M.Person.UserPasswordHash));
            Assert.False(acl.CanExecute(M.Person.Delete));
        }

        [Fact]
        public void PersonIsContactForOtherCustomerOrganisation()
        {
            var otherInternalOrganisation = new OrganisationBuilder(this.Transaction).WithIsInternalOrganisation(true).WithName("other internalOrganisation").Build();
            otherInternalOrganisation.CreateB2BCustomer(new Bogus.Faker());

            this.Transaction.Derive();

            this.Transaction.SetUser(this.Employee);

            var otherCustomer = (Organisation)otherInternalOrganisation.ActiveCustomers.First(v => v.GetType().Name.Equals(typeof(Organisation).Name));
            var contact = otherCustomer.OrganisationContactRelationshipsWhereOrganisation.First().Contact;

            Assert.True(contact.ExistSecurityTokens);

            var acl = new DatabaseAccessControl(this.Security, this.Employee)[contact];
            Assert.True(acl.CanRead(M.Person.FirstName));
            Assert.False(acl.CanWrite(M.Person.FirstName));
            Assert.False(acl.CanRead(M.Person.UserPasswordHash));
            Assert.False(acl.CanWrite(M.Person.UserPasswordHash));
            Assert.False(acl.CanExecute(M.Person.Delete));
        }

        [Fact]
        public void NonUnifiedPartOwnInternalOrganisation()
        {
            var part = new NonUnifiedPartBuilder(this.Transaction).WithName("Name").WithDefaultFacility(this.DefaultFacility).Build();

            this.Transaction.Derive();

            this.Transaction.SetUser(this.Employee);

            Assert.True(part.ExistSecurityTokens);

            var acl = new DatabaseAccessControl(this.Security, this.Employee)[part];
            Assert.True(acl.CanRead(M.NonUnifiedPart.PartSpecifications));
            Assert.False(acl.CanWrite(M.NonUnifiedPart.PartSpecifications));
            Assert.False(acl.CanExecute(M.NonUnifiedPart.Delete));
        }

        [Fact]
        public void NonUnifiedPartOtherInternalOrganisation()
        {
            var otherInternalOrganisation = new OrganisationBuilder(this.Transaction).WithIsInternalOrganisation(true).WithName("other internalOrganisation").WithRequestSequence(new RequestSequences(this.Transaction).EnforcedSequence).Build();
            var otherFacility = new FacilityBuilder(this.Transaction).WithName("other facility").WithOwner(otherInternalOrganisation).WithFacilityType(new FacilityTypes(this.Transaction).Warehouse).Build();

            var part = new NonUnifiedPartBuilder(this.Transaction).WithName("Name").WithDefaultFacility(otherFacility).Build();

            this.Transaction.Derive();

            this.Transaction.SetUser(this.Employee);

            Assert.True(part.ExistSecurityTokens);

            var acl = new DatabaseAccessControl(this.Security, this.Employee)[part];
            Assert.True(acl.CanRead(M.NonUnifiedPart.PartSpecifications));
            Assert.False(acl.CanWrite(M.NonUnifiedPart.PartSpecifications));
            Assert.False(acl.CanExecute(M.NonUnifiedPart.Delete));
        }

        [Fact]
        public void FacilityOwnInternalOrganisation()
        {
            var facility = new FacilityBuilder(this.Transaction).WithName("facility").WithOwner(this.InternalOrganisation).WithFacilityType(new FacilityTypes(this.Transaction).Warehouse).Build();

            this.Transaction.Derive();

            this.Transaction.SetUser(this.Employee);

            Assert.True(facility.ExistSecurityTokens);

            var acl = new DatabaseAccessControl(this.Security, this.Employee)[facility];
            Assert.True(acl.CanRead(M.Facility.Name));
            Assert.False(acl.CanWrite(M.Facility.Name));
        }

        [Fact]
        public void FacilityOtherInternalOrganisation()
        {
            var otherInternalOrganisation = new OrganisationBuilder(this.Transaction).WithIsInternalOrganisation(true).WithName("other internalOrganisation").WithRequestSequence(new RequestSequences(this.Transaction).EnforcedSequence).Build();
            var facility = new FacilityBuilder(this.Transaction).WithName("other facility").WithOwner(otherInternalOrganisation).WithFacilityType(new FacilityTypes(this.Transaction).Warehouse).Build();

            this.Transaction.Derive();

            this.Transaction.SetUser(this.Employee);

            Assert.True(facility.ExistSecurityTokens);

            var acl = new DatabaseAccessControl(this.Security, this.Employee)[facility];
            Assert.True(acl.CanRead(M.Facility.Name));
            Assert.False(acl.CanWrite(M.Facility.Name));
        }

        [Fact]
        public void WorkEffortFixedAssetAssignmentOwnInternalOrganisation()
        {
            var workTask = new WorkTaskBuilder(this.Transaction).WithName("Activity").WithCustomer(this.Customer).WithTakenBy(this.InternalOrganisation).Build();
            var fixedAsset = new SerialisedItemBuilder(this.Transaction).Build();

            this.Transaction.Derive();

            this.Transaction.SetUser(this.Employee);

            var assignment = new WorkEffortFixedAssetAssignmentBuilder(this.Transaction)
                .WithAssignment(workTask)
                .WithFixedAsset(fixedAsset)
                .Build();

            this.Transaction.Derive();

            Assert.True(workTask.ExistSecurityTokens);

            var acl = new DatabaseAccessControl(this.Security, this.Employee)[assignment];
            Assert.True(acl.CanRead(M.WorkEffortFixedAssetAssignment.AllocatedCost));
            Assert.False(acl.CanWrite(M.WorkEffortFixedAssetAssignment.AllocatedCost));
            Assert.False(acl.CanExecute(M.WorkEffortFixedAssetAssignment.Delete));
        }

        [Fact]
        public void WorkEffortFixedAssetAssignmentOtherInternalOrganisation()
        {
            var otherInternalOrganisation = new OrganisationBuilder(this.Transaction).WithIsInternalOrganisation(true).WithName("other internalOrganisation").WithWorkEffortSequence(new WorkEffortSequences(this.Transaction).RestartOnFiscalYear).Build();
            var workTask = new WorkTaskBuilder(this.Transaction).WithName("Activity").WithCustomer(this.Customer).WithTakenBy(otherInternalOrganisation).Build();
            var fixedAsset = new SerialisedItemBuilder(this.Transaction).Build();

            this.Transaction.Derive();

            var assignment = new WorkEffortFixedAssetAssignmentBuilder(this.Transaction)
                .WithAssignment(workTask)
                .WithFixedAsset(fixedAsset)
                .Build();

            this.Transaction.Derive();

            this.Transaction.SetUser(this.Employee);

            Assert.True(workTask.ExistSecurityTokens);

            var acl = new DatabaseAccessControl(this.Security, this.Employee)[assignment];
            Assert.False(acl.CanRead(M.WorkEffortFixedAssetAssignment.AllocatedCost));
            Assert.False(acl.CanWrite(M.WorkEffortFixedAssetAssignment.AllocatedCost));
            Assert.False(acl.CanExecute(M.WorkEffortFixedAssetAssignment.Delete));
        }

        [Fact]
        public void SupplierRelationshipForOwnCustomerOrganisation()
        {
            this.Transaction.SetUser(this.Employee);

            var relationship = this.Supplier.SupplierRelationshipsWhereSupplier.First();

            Assert.True(relationship.ExistSecurityTokens);

            var acl = new DatabaseAccessControl(this.Security, this.Employee)[relationship];
            Assert.True(acl.CanRead(M.SupplierRelationship.NeedsApproval));
            Assert.False(acl.CanWrite(M.SupplierRelationship.NeedsApproval));
            Assert.False(acl.CanExecute(M.SupplierRelationship.Delete));
        }

        [Fact]
        public void SupplierRelationshipForOtherCustomerOrganisation()
        {
            var otherInternalOrganisation = new OrganisationBuilder(this.Transaction).WithIsInternalOrganisation(true).WithName("other internalOrganisation").Build();
            otherInternalOrganisation.CreateSupplier(new Bogus.Faker());

            this.Transaction.Derive();

            this.Transaction.SetUser(this.Employee);

            var relationship = otherInternalOrganisation.SupplierRelationshipsWhereInternalOrganisation.First();

            Assert.True(relationship.ExistSecurityTokens);

            var acl = new DatabaseAccessControl(this.Security, this.Employee)[relationship];
            Assert.False(acl.CanRead(M.SupplierRelationship.NeedsApproval));
            Assert.False(acl.CanWrite(M.SupplierRelationship.NeedsApproval));
            Assert.False(acl.CanExecute(M.SupplierRelationship.Delete));
        }

        [Fact]
        public void SubContractorRelationshipForOwnCustomerOrganisation()
        {
            this.Transaction.SetUser(this.Employee);

            var relationship = this.SubContractor.SubContractorRelationshipsWhereSubContractor.First();

            Assert.True(relationship.ExistSecurityTokens);

            var acl = new DatabaseAccessControl(this.Security, this.Employee)[relationship];
            Assert.True(acl.CanRead(M.SubContractorRelationship.Agreements));
            Assert.False(acl.CanWrite(M.SubContractorRelationship.Agreements));
            Assert.False(acl.CanExecute(M.SubContractorRelationship.Delete));
        }

        [Fact]
        public void SubContractorRelationshipForOtherCustomerOrganisation()
        {
            var otherInternalOrganisation = new OrganisationBuilder(this.Transaction).WithIsInternalOrganisation(true).WithName("other internalOrganisation").Build();
            otherInternalOrganisation.CreateSubContractor(new Bogus.Faker());

            this.Transaction.Derive();

            this.Transaction.SetUser(this.Employee);

            var relationship = otherInternalOrganisation.SubContractorRelationshipsWhereContractor.First();

            Assert.True(relationship.ExistSecurityTokens);

            var acl = new DatabaseAccessControl(this.Security, this.Employee)[relationship];
            Assert.False(acl.CanRead(M.SubContractorRelationship.Agreements));
            Assert.False(acl.CanWrite(M.SubContractorRelationship.Agreements));
            Assert.False(acl.CanExecute(M.SubContractorRelationship.Delete));
        }

        [Fact]
        public void CustomerRelationshipForOwnCustomerOrganisation()
        {
            this.Transaction.SetUser(this.Employee);

            var relationship = this.Customer.CustomerRelationshipsWhereCustomer.First();

            Assert.True(relationship.ExistSecurityTokens);

            var acl = new DatabaseAccessControl(this.Security, this.Employee)[relationship];
            Assert.True(acl.CanRead(M.CustomerRelationship.Agreements));
            Assert.False(acl.CanWrite(M.CustomerRelationship.Agreements));
            Assert.False(acl.CanExecute(M.CustomerRelationship.Delete));
        }

        [Fact]
        public void CustomerRelationshipForOtherCustomerOrganisation()
        {
            var otherInternalOrganisation = new OrganisationBuilder(this.Transaction).WithIsInternalOrganisation(true).WithName("other internalOrganisation").Build();
            otherInternalOrganisation.CreateB2BCustomer(new Bogus.Faker());

            this.Transaction.Derive();

            this.Transaction.SetUser(this.Employee);

            var relationship = otherInternalOrganisation.CustomerRelationshipsWhereInternalOrganisation.First();

            Assert.True(relationship.ExistSecurityTokens);

            var acl = new DatabaseAccessControl(this.Security, this.Employee)[relationship];
            Assert.False(acl.CanRead(M.CustomerRelationship.Agreements));
            Assert.False(acl.CanWrite(M.CustomerRelationship.Agreements));
            Assert.False(acl.CanExecute(M.CustomerRelationship.Delete));
        }

        //TODO: Martien, reactivate test
        [Fact(Skip = "temporary disabled")]
        public void SerialisedItemOwnershipIsTrading()
        {
            var anotherInternalOrganisation = new OrganisationBuilder(this.Transaction).WithInternalOrganisationDefaults().Build();
            var otherEmployee = anotherInternalOrganisation.CreateEmployee("", new Bogus.Faker());

            this.Transaction.Derive();

            var serialisedItem = new SerialisedItemBuilder(this.Transaction)
                .WithOwnership(new Ownerships(this.Transaction).Trading)
                .Build();

            this.Transaction.Derive();

            this.Transaction.SetUser(this.Employee);

            Assert.True(serialisedItem.ExistSecurityTokens);

            var acl = new DatabaseAccessControl(this.Security, this.Employee)[serialisedItem];
            Assert.False(acl.CanRead(M.SerialisedItem.PurchasePrice));
            Assert.False(acl.CanWrite(M.SerialisedItem.PurchasePrice));
            Assert.False(acl.CanRead(M.SerialisedItem.AssignedPurchasePrice));
            Assert.False(acl.CanWrite(M.SerialisedItem.AssignedPurchasePrice));
            Assert.False(acl.CanRead(M.SerialisedItem.EstimatedRefurbishCost));
            Assert.False(acl.CanWrite(M.SerialisedItem.EstimatedRefurbishCost));
            Assert.False(acl.CanRead(M.SerialisedItem.ActualRefurbishCost));
            Assert.False(acl.CanWrite(M.SerialisedItem.ActualRefurbishCost));
            Assert.False(acl.CanRead(M.SerialisedItem.EstimatedTransportCost));
            Assert.False(acl.CanWrite(M.SerialisedItem.EstimatedTransportCost));
            Assert.False(acl.CanRead(M.SerialisedItem.ActualTransportCost));
            Assert.False(acl.CanWrite(M.SerialisedItem.ActualTransportCost));
            Assert.False(acl.CanRead(M.SerialisedItem.ExpectedRentalPriceDryLeaseLongTerm));
            Assert.False(acl.CanWrite(M.SerialisedItem.ExpectedRentalPriceDryLeaseLongTerm));
            Assert.False(acl.CanRead(M.SerialisedItem.ExpectedRentalPriceDryLeaseShortTerm));
            Assert.False(acl.CanWrite(M.SerialisedItem.ExpectedRentalPriceDryLeaseShortTerm));
            Assert.False(acl.CanRead(M.SerialisedItem.ExpectedRentalPriceFullServiceLongTerm));
            Assert.False(acl.CanWrite(M.SerialisedItem.ExpectedRentalPriceFullServiceLongTerm));
            Assert.False(acl.CanRead(M.SerialisedItem.ExpectedRentalPriceFullServiceShortTerm));
            Assert.False(acl.CanWrite(M.SerialisedItem.ExpectedRentalPriceFullServiceShortTerm));
            Assert.False(acl.CanRead(M.SerialisedItem.FromInitialImport));
            Assert.False(acl.CanWrite(M.SerialisedItem.FromInitialImport));

            Assert.False(acl.CanRead(M.SerialisedItem.SerialNumber));
            Assert.False(acl.CanWrite(M.SerialisedItem.SerialNumber));
            Assert.False(acl.CanExecute(M.SerialisedItem.Delete));

            Assert.False(acl.CanRead(M.SerialisedItem.PrimaryPhoto));
            Assert.False(acl.CanWrite(M.SerialisedItem.PrimaryPhoto));
            Assert.False(acl.CanRead(M.SerialisedItem.SecondaryPhotos));
            Assert.False(acl.CanWrite(M.SerialisedItem.SecondaryPhotos));
            Assert.False(acl.CanRead(M.SerialisedItem.AdditionalPhotos));
            Assert.False(acl.CanWrite(M.SerialisedItem.AdditionalPhotos));
            Assert.False(acl.CanRead(M.SerialisedItem.PrivatePhotos));
            Assert.False(acl.CanWrite(M.SerialisedItem.PrivatePhotos));
            Assert.False(acl.CanRead(M.SerialisedItem.PublicElectronicDocuments));
            Assert.False(acl.CanWrite(M.SerialisedItem.PublicElectronicDocuments));
            Assert.False(acl.CanRead(M.SerialisedItem.PublicLocalisedElectronicDocuments));
            Assert.False(acl.CanWrite(M.SerialisedItem.PublicLocalisedElectronicDocuments));
            Assert.False(acl.CanRead(M.SerialisedItem.PrivateElectronicDocuments));
            Assert.False(acl.CanWrite(M.SerialisedItem.PrivateElectronicDocuments));
            Assert.False(acl.CanRead(M.SerialisedItem.PrivateLocalisedElectronicDocuments));
            Assert.False(acl.CanWrite(M.SerialisedItem.PrivateLocalisedElectronicDocuments));
            Assert.False(acl.CanRead(M.SerialisedItem.SerialisedItemCharacteristics));
            Assert.False(acl.CanWrite(M.SerialisedItem.SerialisedItemCharacteristics));

            this.Transaction.SetUser(otherEmployee);

            Assert.True(serialisedItem.ExistSecurityTokens);

            acl = new DatabaseAccessControl(this.Security, otherEmployee)[serialisedItem];

            Assert.False(acl.CanRead(M.SerialisedItem.PurchasePrice));
            Assert.False(acl.CanWrite(M.SerialisedItem.PurchasePrice));
            Assert.False(acl.CanRead(M.SerialisedItem.AssignedPurchasePrice));
            Assert.False(acl.CanWrite(M.SerialisedItem.AssignedPurchasePrice));
            Assert.False(acl.CanRead(M.SerialisedItem.EstimatedRefurbishCost));
            Assert.False(acl.CanWrite(M.SerialisedItem.EstimatedRefurbishCost));
            Assert.False(acl.CanRead(M.SerialisedItem.ActualRefurbishCost));
            Assert.False(acl.CanWrite(M.SerialisedItem.ActualRefurbishCost));
            Assert.False(acl.CanRead(M.SerialisedItem.EstimatedTransportCost));
            Assert.False(acl.CanWrite(M.SerialisedItem.EstimatedTransportCost));
            Assert.False(acl.CanRead(M.SerialisedItem.ActualTransportCost));
            Assert.False(acl.CanWrite(M.SerialisedItem.ActualTransportCost));
            Assert.False(acl.CanRead(M.SerialisedItem.ExpectedRentalPriceDryLeaseLongTerm));
            Assert.False(acl.CanWrite(M.SerialisedItem.ExpectedRentalPriceDryLeaseLongTerm));
            Assert.False(acl.CanRead(M.SerialisedItem.ExpectedRentalPriceDryLeaseShortTerm));
            Assert.False(acl.CanWrite(M.SerialisedItem.ExpectedRentalPriceDryLeaseShortTerm));
            Assert.False(acl.CanRead(M.SerialisedItem.ExpectedRentalPriceFullServiceLongTerm));
            Assert.False(acl.CanWrite(M.SerialisedItem.ExpectedRentalPriceFullServiceLongTerm));
            Assert.False(acl.CanRead(M.SerialisedItem.ExpectedRentalPriceFullServiceShortTerm));
            Assert.False(acl.CanWrite(M.SerialisedItem.ExpectedRentalPriceFullServiceShortTerm));
            Assert.False(acl.CanRead(M.SerialisedItem.FromInitialImport));
            Assert.False(acl.CanWrite(M.SerialisedItem.FromInitialImport));

            Assert.True(acl.CanRead(M.SerialisedItem.SerialNumber));
            Assert.False(acl.CanWrite(M.SerialisedItem.SerialNumber));
            Assert.False(acl.CanExecute(M.SerialisedItem.Delete));

            Assert.True(acl.CanRead(M.SerialisedItem.PrimaryPhoto));
            Assert.True(acl.CanWrite(M.SerialisedItem.PrimaryPhoto));
            Assert.True(acl.CanRead(M.SerialisedItem.SecondaryPhotos));
            Assert.True(acl.CanWrite(M.SerialisedItem.SecondaryPhotos));
            Assert.True(acl.CanRead(M.SerialisedItem.AdditionalPhotos));
            Assert.True(acl.CanWrite(M.SerialisedItem.AdditionalPhotos));
            Assert.True(acl.CanRead(M.SerialisedItem.PrivatePhotos));
            Assert.True(acl.CanWrite(M.SerialisedItem.PrivatePhotos));
            Assert.True(acl.CanRead(M.SerialisedItem.PublicElectronicDocuments));
            Assert.True(acl.CanWrite(M.SerialisedItem.PublicElectronicDocuments));
            Assert.True(acl.CanRead(M.SerialisedItem.PublicLocalisedElectronicDocuments));
            Assert.True(acl.CanWrite(M.SerialisedItem.PublicLocalisedElectronicDocuments));
            Assert.True(acl.CanRead(M.SerialisedItem.PrivateElectronicDocuments));
            Assert.True(acl.CanWrite(M.SerialisedItem.PrivateElectronicDocuments));
            Assert.True(acl.CanRead(M.SerialisedItem.PrivateLocalisedElectronicDocuments));
            Assert.True(acl.CanWrite(M.SerialisedItem.PrivateLocalisedElectronicDocuments));
            Assert.True(acl.CanRead(M.SerialisedItem.SerialisedItemCharacteristics));
            Assert.True(acl.CanWrite(M.SerialisedItem.SerialisedItemCharacteristics));
        }

        [Fact]
        public void RequestItemSerialisedItemOwnInternalOrganisation()
        {
            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();

            var request = new RequestForQuoteBuilder(this.Transaction)
                .WithRecipient(this.InternalOrganisation)
                .Build();

            var requestItem = new RequestItemBuilder(this.Transaction)
                .WithSerialisedItem(serialisedItem)
                .WithQuantity(1)
                .Build();

            request.AddRequestItem(requestItem);

            this.Transaction.Derive();

            this.Transaction.SetUser(this.Employee);

            Assert.True(serialisedItem.ExistSecurityTokens);

            var acl = new DatabaseAccessControl(this.Security, this.Employee)[serialisedItem];
            Assert.False(acl.CanRead(M.SerialisedItem.PurchasePrice));
            Assert.False(acl.CanWrite(M.SerialisedItem.PurchasePrice));
            Assert.False(acl.CanRead(M.SerialisedItem.AssignedPurchasePrice));
            Assert.False(acl.CanWrite(M.SerialisedItem.AssignedPurchasePrice));
            Assert.False(acl.CanRead(M.SerialisedItem.EstimatedRefurbishCost));
            Assert.False(acl.CanWrite(M.SerialisedItem.EstimatedRefurbishCost));
            Assert.False(acl.CanRead(M.SerialisedItem.ActualRefurbishCost));
            Assert.False(acl.CanWrite(M.SerialisedItem.ActualRefurbishCost));
            Assert.False(acl.CanRead(M.SerialisedItem.EstimatedTransportCost));
            Assert.False(acl.CanWrite(M.SerialisedItem.EstimatedTransportCost));
            Assert.False(acl.CanRead(M.SerialisedItem.ActualTransportCost));
            Assert.False(acl.CanWrite(M.SerialisedItem.ActualTransportCost));
            Assert.False(acl.CanRead(M.SerialisedItem.ExpectedRentalPriceDryLeaseLongTerm));
            Assert.False(acl.CanWrite(M.SerialisedItem.ExpectedRentalPriceDryLeaseLongTerm));
            Assert.False(acl.CanRead(M.SerialisedItem.ExpectedRentalPriceDryLeaseShortTerm));
            Assert.False(acl.CanWrite(M.SerialisedItem.ExpectedRentalPriceDryLeaseShortTerm));
            Assert.False(acl.CanRead(M.SerialisedItem.ExpectedRentalPriceFullServiceLongTerm));
            Assert.False(acl.CanWrite(M.SerialisedItem.ExpectedRentalPriceFullServiceLongTerm));
            Assert.False(acl.CanRead(M.SerialisedItem.ExpectedRentalPriceFullServiceShortTerm));
            Assert.False(acl.CanWrite(M.SerialisedItem.ExpectedRentalPriceFullServiceShortTerm));
            Assert.False(acl.CanRead(M.SerialisedItem.FromInitialImport));
            Assert.False(acl.CanWrite(M.SerialisedItem.FromInitialImport));

            Assert.True(acl.CanRead(M.SerialisedItem.SerialNumber));
            Assert.False(acl.CanWrite(M.SerialisedItem.SerialNumber));
            Assert.False(acl.CanExecute(M.SerialisedItem.Delete));

            Assert.True(acl.CanRead(M.SerialisedItem.PrimaryPhoto));
            Assert.True(acl.CanWrite(M.SerialisedItem.PrimaryPhoto));
            Assert.True(acl.CanRead(M.SerialisedItem.SecondaryPhotos));
            Assert.True(acl.CanWrite(M.SerialisedItem.SecondaryPhotos));
            Assert.True(acl.CanRead(M.SerialisedItem.AdditionalPhotos));
            Assert.True(acl.CanWrite(M.SerialisedItem.AdditionalPhotos));
            Assert.True(acl.CanRead(M.SerialisedItem.PrivatePhotos));
            Assert.True(acl.CanWrite(M.SerialisedItem.PrivatePhotos));
            Assert.True(acl.CanRead(M.SerialisedItem.PublicElectronicDocuments));
            Assert.True(acl.CanWrite(M.SerialisedItem.PublicElectronicDocuments));
            Assert.True(acl.CanRead(M.SerialisedItem.PublicLocalisedElectronicDocuments));
            Assert.True(acl.CanWrite(M.SerialisedItem.PublicLocalisedElectronicDocuments));
            Assert.True(acl.CanRead(M.SerialisedItem.PrivateElectronicDocuments));
            Assert.True(acl.CanWrite(M.SerialisedItem.PrivateElectronicDocuments));
            Assert.True(acl.CanRead(M.SerialisedItem.PrivateLocalisedElectronicDocuments));
            Assert.True(acl.CanWrite(M.SerialisedItem.PrivateLocalisedElectronicDocuments));
            Assert.True(acl.CanRead(M.SerialisedItem.SerialisedItemCharacteristics));
            Assert.True(acl.CanWrite(M.SerialisedItem.SerialisedItemCharacteristics));
        }

        [Fact]
        public void QuoteItemSerialisedItemOwnInternalOrganisation()
        {
            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();

            var quote = new ProductQuoteBuilder(this.Transaction)
                .WithIssuer(this.InternalOrganisation)
                .Build();

            var quoteItem = new QuoteItemBuilder(this.Transaction)
                .WithSerialisedItem(serialisedItem)
                .WithQuantity(1)
                .Build();

            quote.AddQuoteItem(quoteItem);

            this.Transaction.Derive(false);

            this.Transaction.SetUser(this.Employee);

            Assert.True(serialisedItem.ExistSecurityTokens);

            var acl = new DatabaseAccessControl(this.Security, this.Employee)[serialisedItem];
            Assert.False(acl.CanRead(M.SerialisedItem.PurchasePrice));
            Assert.False(acl.CanWrite(M.SerialisedItem.PurchasePrice));
            Assert.False(acl.CanRead(M.SerialisedItem.AssignedPurchasePrice));
            Assert.False(acl.CanWrite(M.SerialisedItem.AssignedPurchasePrice));
            Assert.False(acl.CanRead(M.SerialisedItem.EstimatedRefurbishCost));
            Assert.False(acl.CanWrite(M.SerialisedItem.EstimatedRefurbishCost));
            Assert.False(acl.CanRead(M.SerialisedItem.ActualRefurbishCost));
            Assert.False(acl.CanWrite(M.SerialisedItem.ActualRefurbishCost));
            Assert.False(acl.CanRead(M.SerialisedItem.EstimatedTransportCost));
            Assert.False(acl.CanWrite(M.SerialisedItem.EstimatedTransportCost));
            Assert.False(acl.CanRead(M.SerialisedItem.ActualTransportCost));
            Assert.False(acl.CanWrite(M.SerialisedItem.ActualTransportCost));
            Assert.False(acl.CanRead(M.SerialisedItem.ExpectedRentalPriceDryLeaseLongTerm));
            Assert.False(acl.CanWrite(M.SerialisedItem.ExpectedRentalPriceDryLeaseLongTerm));
            Assert.False(acl.CanRead(M.SerialisedItem.ExpectedRentalPriceDryLeaseShortTerm));
            Assert.False(acl.CanWrite(M.SerialisedItem.ExpectedRentalPriceDryLeaseShortTerm));
            Assert.False(acl.CanRead(M.SerialisedItem.ExpectedRentalPriceFullServiceLongTerm));
            Assert.False(acl.CanWrite(M.SerialisedItem.ExpectedRentalPriceFullServiceLongTerm));
            Assert.False(acl.CanRead(M.SerialisedItem.ExpectedRentalPriceFullServiceShortTerm));
            Assert.False(acl.CanWrite(M.SerialisedItem.ExpectedRentalPriceFullServiceShortTerm));
            Assert.False(acl.CanRead(M.SerialisedItem.FromInitialImport));
            Assert.False(acl.CanWrite(M.SerialisedItem.FromInitialImport));

            Assert.True(acl.CanRead(M.SerialisedItem.SerialNumber));
            Assert.False(acl.CanWrite(M.SerialisedItem.SerialNumber));
            Assert.False(acl.CanExecute(M.SerialisedItem.Delete));

            Assert.True(acl.CanRead(M.SerialisedItem.PrimaryPhoto));
            Assert.True(acl.CanWrite(M.SerialisedItem.PrimaryPhoto));
            Assert.True(acl.CanRead(M.SerialisedItem.SecondaryPhotos));
            Assert.True(acl.CanWrite(M.SerialisedItem.SecondaryPhotos));
            Assert.True(acl.CanRead(M.SerialisedItem.AdditionalPhotos));
            Assert.True(acl.CanWrite(M.SerialisedItem.AdditionalPhotos));
            Assert.True(acl.CanRead(M.SerialisedItem.PrivatePhotos));
            Assert.True(acl.CanWrite(M.SerialisedItem.PrivatePhotos));
            Assert.True(acl.CanRead(M.SerialisedItem.PublicElectronicDocuments));
            Assert.True(acl.CanWrite(M.SerialisedItem.PublicElectronicDocuments));
            Assert.True(acl.CanRead(M.SerialisedItem.PublicLocalisedElectronicDocuments));
            Assert.True(acl.CanWrite(M.SerialisedItem.PublicLocalisedElectronicDocuments));
            Assert.True(acl.CanRead(M.SerialisedItem.PrivateElectronicDocuments));
            Assert.True(acl.CanWrite(M.SerialisedItem.PrivateElectronicDocuments));
            Assert.True(acl.CanRead(M.SerialisedItem.PrivateLocalisedElectronicDocuments));
            Assert.True(acl.CanWrite(M.SerialisedItem.PrivateLocalisedElectronicDocuments));
            Assert.True(acl.CanRead(M.SerialisedItem.SerialisedItemCharacteristics));
            Assert.True(acl.CanWrite(M.SerialisedItem.SerialisedItemCharacteristics));
        }

        [Fact]
        public void PurchaseOrderItemSerialisedItemOwnInternalOrganisation()
        {
            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();

            var order = new PurchaseOrderBuilder(this.Transaction)
                .WithOrderedBy(this.InternalOrganisation)
                .Build();

            var orderItem = new PurchaseOrderItemBuilder(this.Transaction)
                .WithSerialisedItem(serialisedItem)
                .Build();

            order.AddPurchaseOrderItem(orderItem);

            this.Transaction.Derive(false);

            this.Transaction.SetUser(this.Employee);

            Assert.True(serialisedItem.ExistSecurityTokens);

            var acl = new DatabaseAccessControl(this.Security, this.Employee)[serialisedItem];
            Assert.False(acl.CanRead(M.SerialisedItem.PurchasePrice));
            Assert.False(acl.CanWrite(M.SerialisedItem.PurchasePrice));
            Assert.False(acl.CanRead(M.SerialisedItem.AssignedPurchasePrice));
            Assert.False(acl.CanWrite(M.SerialisedItem.AssignedPurchasePrice));
            Assert.False(acl.CanRead(M.SerialisedItem.EstimatedRefurbishCost));
            Assert.False(acl.CanWrite(M.SerialisedItem.EstimatedRefurbishCost));
            Assert.False(acl.CanRead(M.SerialisedItem.ActualRefurbishCost));
            Assert.False(acl.CanWrite(M.SerialisedItem.ActualRefurbishCost));
            Assert.False(acl.CanRead(M.SerialisedItem.EstimatedTransportCost));
            Assert.False(acl.CanWrite(M.SerialisedItem.EstimatedTransportCost));
            Assert.False(acl.CanRead(M.SerialisedItem.ActualTransportCost));
            Assert.False(acl.CanWrite(M.SerialisedItem.ActualTransportCost));
            Assert.False(acl.CanRead(M.SerialisedItem.ExpectedRentalPriceDryLeaseLongTerm));
            Assert.False(acl.CanWrite(M.SerialisedItem.ExpectedRentalPriceDryLeaseLongTerm));
            Assert.False(acl.CanRead(M.SerialisedItem.ExpectedRentalPriceDryLeaseShortTerm));
            Assert.False(acl.CanWrite(M.SerialisedItem.ExpectedRentalPriceDryLeaseShortTerm));
            Assert.False(acl.CanRead(M.SerialisedItem.ExpectedRentalPriceFullServiceLongTerm));
            Assert.False(acl.CanWrite(M.SerialisedItem.ExpectedRentalPriceFullServiceLongTerm));
            Assert.False(acl.CanRead(M.SerialisedItem.ExpectedRentalPriceFullServiceShortTerm));
            Assert.False(acl.CanWrite(M.SerialisedItem.ExpectedRentalPriceFullServiceShortTerm));
            Assert.False(acl.CanRead(M.SerialisedItem.FromInitialImport));
            Assert.False(acl.CanWrite(M.SerialisedItem.FromInitialImport));

            Assert.True(acl.CanRead(M.SerialisedItem.SerialNumber));
            Assert.False(acl.CanWrite(M.SerialisedItem.SerialNumber));
            Assert.False(acl.CanExecute(M.SerialisedItem.Delete));

            Assert.True(acl.CanRead(M.SerialisedItem.PrimaryPhoto));
            Assert.True(acl.CanWrite(M.SerialisedItem.PrimaryPhoto));
            Assert.True(acl.CanRead(M.SerialisedItem.SecondaryPhotos));
            Assert.True(acl.CanWrite(M.SerialisedItem.SecondaryPhotos));
            Assert.True(acl.CanRead(M.SerialisedItem.AdditionalPhotos));
            Assert.True(acl.CanWrite(M.SerialisedItem.AdditionalPhotos));
            Assert.True(acl.CanRead(M.SerialisedItem.PrivatePhotos));
            Assert.True(acl.CanWrite(M.SerialisedItem.PrivatePhotos));
            Assert.True(acl.CanRead(M.SerialisedItem.PublicElectronicDocuments));
            Assert.True(acl.CanWrite(M.SerialisedItem.PublicElectronicDocuments));
            Assert.True(acl.CanRead(M.SerialisedItem.PublicLocalisedElectronicDocuments));
            Assert.True(acl.CanWrite(M.SerialisedItem.PublicLocalisedElectronicDocuments));
            Assert.True(acl.CanRead(M.SerialisedItem.PrivateElectronicDocuments));
            Assert.True(acl.CanWrite(M.SerialisedItem.PrivateElectronicDocuments));
            Assert.True(acl.CanRead(M.SerialisedItem.PrivateLocalisedElectronicDocuments));
            Assert.True(acl.CanWrite(M.SerialisedItem.PrivateLocalisedElectronicDocuments));
            Assert.True(acl.CanRead(M.SerialisedItem.SerialisedItemCharacteristics));
            Assert.True(acl.CanWrite(M.SerialisedItem.SerialisedItemCharacteristics));
        }

        [Fact]
        public void PurchaseInvoiceItemSerialisedItemOwnInternalOrganisation()
        {
            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();

            var invoice = new PurchaseInvoiceBuilder(this.Transaction)
                .WithBilledTo(this.InternalOrganisation)
                .Build();

            var invoiceItem = new PurchaseInvoiceItemBuilder(this.Transaction)
                .WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).PartItem)
                .WithSerialisedItem(serialisedItem)
                .Build();

            invoice.AddPurchaseInvoiceItem(invoiceItem);

            this.Transaction.Derive(false);

            this.Transaction.SetUser(this.Employee);

            Assert.True(serialisedItem.ExistSecurityTokens);

            var acl = new DatabaseAccessControl(this.Security, this.Employee)[serialisedItem];
            Assert.False(acl.CanRead(M.SerialisedItem.PurchasePrice));
            Assert.False(acl.CanWrite(M.SerialisedItem.PurchasePrice));
            Assert.False(acl.CanRead(M.SerialisedItem.AssignedPurchasePrice));
            Assert.False(acl.CanWrite(M.SerialisedItem.AssignedPurchasePrice));
            Assert.False(acl.CanRead(M.SerialisedItem.EstimatedRefurbishCost));
            Assert.False(acl.CanWrite(M.SerialisedItem.EstimatedRefurbishCost));
            Assert.False(acl.CanRead(M.SerialisedItem.ActualRefurbishCost));
            Assert.False(acl.CanWrite(M.SerialisedItem.ActualRefurbishCost));
            Assert.False(acl.CanRead(M.SerialisedItem.EstimatedTransportCost));
            Assert.False(acl.CanWrite(M.SerialisedItem.EstimatedTransportCost));
            Assert.False(acl.CanRead(M.SerialisedItem.ActualTransportCost));
            Assert.False(acl.CanWrite(M.SerialisedItem.ActualTransportCost));
            Assert.False(acl.CanRead(M.SerialisedItem.ExpectedRentalPriceDryLeaseLongTerm));
            Assert.False(acl.CanWrite(M.SerialisedItem.ExpectedRentalPriceDryLeaseLongTerm));
            Assert.False(acl.CanRead(M.SerialisedItem.ExpectedRentalPriceDryLeaseShortTerm));
            Assert.False(acl.CanWrite(M.SerialisedItem.ExpectedRentalPriceDryLeaseShortTerm));
            Assert.False(acl.CanRead(M.SerialisedItem.ExpectedRentalPriceFullServiceLongTerm));
            Assert.False(acl.CanWrite(M.SerialisedItem.ExpectedRentalPriceFullServiceLongTerm));
            Assert.False(acl.CanRead(M.SerialisedItem.ExpectedRentalPriceFullServiceShortTerm));
            Assert.False(acl.CanWrite(M.SerialisedItem.ExpectedRentalPriceFullServiceShortTerm));
            Assert.False(acl.CanRead(M.SerialisedItem.FromInitialImport));
            Assert.False(acl.CanWrite(M.SerialisedItem.FromInitialImport));

            Assert.True(acl.CanRead(M.SerialisedItem.SerialNumber));
            Assert.False(acl.CanWrite(M.SerialisedItem.SerialNumber));
            Assert.False(acl.CanExecute(M.SerialisedItem.Delete));

            Assert.True(acl.CanRead(M.SerialisedItem.PrimaryPhoto));
            Assert.True(acl.CanWrite(M.SerialisedItem.PrimaryPhoto));
            Assert.True(acl.CanRead(M.SerialisedItem.SecondaryPhotos));
            Assert.True(acl.CanWrite(M.SerialisedItem.SecondaryPhotos));
            Assert.True(acl.CanRead(M.SerialisedItem.AdditionalPhotos));
            Assert.True(acl.CanWrite(M.SerialisedItem.AdditionalPhotos));
            Assert.True(acl.CanRead(M.SerialisedItem.PrivatePhotos));
            Assert.True(acl.CanWrite(M.SerialisedItem.PrivatePhotos));
            Assert.True(acl.CanRead(M.SerialisedItem.PublicElectronicDocuments));
            Assert.True(acl.CanWrite(M.SerialisedItem.PublicElectronicDocuments));
            Assert.True(acl.CanRead(M.SerialisedItem.PublicLocalisedElectronicDocuments));
            Assert.True(acl.CanWrite(M.SerialisedItem.PublicLocalisedElectronicDocuments));
            Assert.True(acl.CanRead(M.SerialisedItem.PrivateElectronicDocuments));
            Assert.True(acl.CanWrite(M.SerialisedItem.PrivateElectronicDocuments));
            Assert.True(acl.CanRead(M.SerialisedItem.PrivateLocalisedElectronicDocuments));
            Assert.True(acl.CanWrite(M.SerialisedItem.PrivateLocalisedElectronicDocuments));
            Assert.True(acl.CanRead(M.SerialisedItem.SerialisedItemCharacteristics));
            Assert.True(acl.CanWrite(M.SerialisedItem.SerialisedItemCharacteristics));
        }

        [Fact]
        public void SalesOrderItemSerialisedItemOwnInternalOrganisation()
        {
            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();

            var order = new SalesOrderBuilder(this.Transaction)
                .WithTakenBy(this.InternalOrganisation)
                .Build();

            var orderItem = new SalesOrderItemBuilder(this.Transaction)
                .WithSerialisedItem(serialisedItem)
                .Build();

            order.AddSalesOrderItem(orderItem);

            this.Transaction.Derive(false);

            this.Transaction.SetUser(this.Employee);

            Assert.True(serialisedItem.ExistSecurityTokens);

            var acl = new DatabaseAccessControl(this.Security, this.Employee)[serialisedItem];
            Assert.False(acl.CanRead(M.SerialisedItem.PurchasePrice));
            Assert.False(acl.CanWrite(M.SerialisedItem.PurchasePrice));
            Assert.False(acl.CanRead(M.SerialisedItem.AssignedPurchasePrice));
            Assert.False(acl.CanWrite(M.SerialisedItem.AssignedPurchasePrice));
            Assert.False(acl.CanRead(M.SerialisedItem.EstimatedRefurbishCost));
            Assert.False(acl.CanWrite(M.SerialisedItem.EstimatedRefurbishCost));
            Assert.False(acl.CanRead(M.SerialisedItem.ActualRefurbishCost));
            Assert.False(acl.CanWrite(M.SerialisedItem.ActualRefurbishCost));
            Assert.False(acl.CanRead(M.SerialisedItem.EstimatedTransportCost));
            Assert.False(acl.CanWrite(M.SerialisedItem.EstimatedTransportCost));
            Assert.False(acl.CanRead(M.SerialisedItem.ActualTransportCost));
            Assert.False(acl.CanWrite(M.SerialisedItem.ActualTransportCost));
            Assert.False(acl.CanRead(M.SerialisedItem.ExpectedRentalPriceDryLeaseLongTerm));
            Assert.False(acl.CanWrite(M.SerialisedItem.ExpectedRentalPriceDryLeaseLongTerm));
            Assert.False(acl.CanRead(M.SerialisedItem.ExpectedRentalPriceDryLeaseShortTerm));
            Assert.False(acl.CanWrite(M.SerialisedItem.ExpectedRentalPriceDryLeaseShortTerm));
            Assert.False(acl.CanRead(M.SerialisedItem.ExpectedRentalPriceFullServiceLongTerm));
            Assert.False(acl.CanWrite(M.SerialisedItem.ExpectedRentalPriceFullServiceLongTerm));
            Assert.False(acl.CanRead(M.SerialisedItem.ExpectedRentalPriceFullServiceShortTerm));
            Assert.False(acl.CanWrite(M.SerialisedItem.ExpectedRentalPriceFullServiceShortTerm));
            Assert.False(acl.CanRead(M.SerialisedItem.FromInitialImport));
            Assert.False(acl.CanWrite(M.SerialisedItem.FromInitialImport));

            Assert.True(acl.CanRead(M.SerialisedItem.SerialNumber));
            Assert.False(acl.CanWrite(M.SerialisedItem.SerialNumber));
            Assert.False(acl.CanExecute(M.SerialisedItem.Delete));

            Assert.True(acl.CanRead(M.SerialisedItem.PrimaryPhoto));
            Assert.True(acl.CanWrite(M.SerialisedItem.PrimaryPhoto));
            Assert.True(acl.CanRead(M.SerialisedItem.SecondaryPhotos));
            Assert.True(acl.CanWrite(M.SerialisedItem.SecondaryPhotos));
            Assert.True(acl.CanRead(M.SerialisedItem.AdditionalPhotos));
            Assert.True(acl.CanWrite(M.SerialisedItem.AdditionalPhotos));
            Assert.True(acl.CanRead(M.SerialisedItem.PrivatePhotos));
            Assert.True(acl.CanWrite(M.SerialisedItem.PrivatePhotos));
            Assert.True(acl.CanRead(M.SerialisedItem.PublicElectronicDocuments));
            Assert.True(acl.CanWrite(M.SerialisedItem.PublicElectronicDocuments));
            Assert.True(acl.CanRead(M.SerialisedItem.PublicLocalisedElectronicDocuments));
            Assert.True(acl.CanWrite(M.SerialisedItem.PublicLocalisedElectronicDocuments));
            Assert.True(acl.CanRead(M.SerialisedItem.PrivateElectronicDocuments));
            Assert.True(acl.CanWrite(M.SerialisedItem.PrivateElectronicDocuments));
            Assert.True(acl.CanRead(M.SerialisedItem.PrivateLocalisedElectronicDocuments));
            Assert.True(acl.CanWrite(M.SerialisedItem.PrivateLocalisedElectronicDocuments));
            Assert.True(acl.CanRead(M.SerialisedItem.SerialisedItemCharacteristics));
            Assert.True(acl.CanWrite(M.SerialisedItem.SerialisedItemCharacteristics));
        }

        [Fact]
        public void SalesInvoiceItemSerialisedItemOwnInternalOrganisation()
        {
            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();

            var invoice = new SalesInvoiceBuilder(this.Transaction)
                .WithBilledFrom(this.InternalOrganisation)
                .Build();

            var invoiceItem = new SalesInvoiceItemBuilder(this.Transaction)
                .WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).PartItem)
                .WithSerialisedItem(serialisedItem)
                .Build();

            invoice.AddSalesInvoiceItem(invoiceItem);

            this.Transaction.Derive(false);

            this.Transaction.SetUser(this.Employee);

            Assert.True(serialisedItem.ExistSecurityTokens);

            var acl = new DatabaseAccessControl(this.Security, this.Employee)[serialisedItem];
            Assert.False(acl.CanRead(M.SerialisedItem.PurchasePrice));
            Assert.False(acl.CanWrite(M.SerialisedItem.PurchasePrice));
            Assert.False(acl.CanRead(M.SerialisedItem.AssignedPurchasePrice));
            Assert.False(acl.CanWrite(M.SerialisedItem.AssignedPurchasePrice));
            Assert.False(acl.CanRead(M.SerialisedItem.EstimatedRefurbishCost));
            Assert.False(acl.CanWrite(M.SerialisedItem.EstimatedRefurbishCost));
            Assert.False(acl.CanRead(M.SerialisedItem.ActualRefurbishCost));
            Assert.False(acl.CanWrite(M.SerialisedItem.ActualRefurbishCost));
            Assert.False(acl.CanRead(M.SerialisedItem.EstimatedTransportCost));
            Assert.False(acl.CanWrite(M.SerialisedItem.EstimatedTransportCost));
            Assert.False(acl.CanRead(M.SerialisedItem.ActualTransportCost));
            Assert.False(acl.CanWrite(M.SerialisedItem.ActualTransportCost));
            Assert.False(acl.CanRead(M.SerialisedItem.ExpectedRentalPriceDryLeaseLongTerm));
            Assert.False(acl.CanWrite(M.SerialisedItem.ExpectedRentalPriceDryLeaseLongTerm));
            Assert.False(acl.CanRead(M.SerialisedItem.ExpectedRentalPriceDryLeaseShortTerm));
            Assert.False(acl.CanWrite(M.SerialisedItem.ExpectedRentalPriceDryLeaseShortTerm));
            Assert.False(acl.CanRead(M.SerialisedItem.ExpectedRentalPriceFullServiceLongTerm));
            Assert.False(acl.CanWrite(M.SerialisedItem.ExpectedRentalPriceFullServiceLongTerm));
            Assert.False(acl.CanRead(M.SerialisedItem.ExpectedRentalPriceFullServiceShortTerm));
            Assert.False(acl.CanWrite(M.SerialisedItem.ExpectedRentalPriceFullServiceShortTerm));
            Assert.False(acl.CanRead(M.SerialisedItem.FromInitialImport));
            Assert.False(acl.CanWrite(M.SerialisedItem.FromInitialImport));

            Assert.True(acl.CanRead(M.SerialisedItem.SerialNumber));
            Assert.False(acl.CanWrite(M.SerialisedItem.SerialNumber));
            Assert.False(acl.CanExecute(M.SerialisedItem.Delete));

            Assert.True(acl.CanRead(M.SerialisedItem.PrimaryPhoto));
            Assert.True(acl.CanWrite(M.SerialisedItem.PrimaryPhoto));
            Assert.True(acl.CanRead(M.SerialisedItem.SecondaryPhotos));
            Assert.True(acl.CanWrite(M.SerialisedItem.SecondaryPhotos));
            Assert.True(acl.CanRead(M.SerialisedItem.AdditionalPhotos));
            Assert.True(acl.CanWrite(M.SerialisedItem.AdditionalPhotos));
            Assert.True(acl.CanRead(M.SerialisedItem.PrivatePhotos));
            Assert.True(acl.CanWrite(M.SerialisedItem.PrivatePhotos));
            Assert.True(acl.CanRead(M.SerialisedItem.PublicElectronicDocuments));
            Assert.True(acl.CanWrite(M.SerialisedItem.PublicElectronicDocuments));
            Assert.True(acl.CanRead(M.SerialisedItem.PublicLocalisedElectronicDocuments));
            Assert.True(acl.CanWrite(M.SerialisedItem.PublicLocalisedElectronicDocuments));
            Assert.True(acl.CanRead(M.SerialisedItem.PrivateElectronicDocuments));
            Assert.True(acl.CanWrite(M.SerialisedItem.PrivateElectronicDocuments));
            Assert.True(acl.CanRead(M.SerialisedItem.PrivateLocalisedElectronicDocuments));
            Assert.True(acl.CanWrite(M.SerialisedItem.PrivateLocalisedElectronicDocuments));
            Assert.True(acl.CanRead(M.SerialisedItem.SerialisedItemCharacteristics));
            Assert.True(acl.CanWrite(M.SerialisedItem.SerialisedItemCharacteristics));
        }

        [Fact]
        public void CustomerShipmentItemSerialisedItemOwnInternalOrganisation()
        {
            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();

            var shipment = new CustomerShipmentBuilder(this.Transaction)
                .WithShipFromParty(this.InternalOrganisation)
                .Build();

            var shipmentItem = new ShipmentItemBuilder(this.Transaction)
                .WithSerialisedItem(serialisedItem)
                .Build();

            shipment.AddShipmentItem(shipmentItem);

            this.Transaction.Derive(false);

            this.Transaction.SetUser(this.Employee);

            Assert.True(serialisedItem.ExistSecurityTokens);

            var acl = new DatabaseAccessControl(this.Security, this.Employee)[serialisedItem];
            Assert.False(acl.CanRead(M.SerialisedItem.PurchasePrice));
            Assert.False(acl.CanWrite(M.SerialisedItem.PurchasePrice));
            Assert.False(acl.CanRead(M.SerialisedItem.AssignedPurchasePrice));
            Assert.False(acl.CanWrite(M.SerialisedItem.AssignedPurchasePrice));
            Assert.False(acl.CanRead(M.SerialisedItem.EstimatedRefurbishCost));
            Assert.False(acl.CanWrite(M.SerialisedItem.EstimatedRefurbishCost));
            Assert.False(acl.CanRead(M.SerialisedItem.ActualRefurbishCost));
            Assert.False(acl.CanWrite(M.SerialisedItem.ActualRefurbishCost));
            Assert.False(acl.CanRead(M.SerialisedItem.EstimatedTransportCost));
            Assert.False(acl.CanWrite(M.SerialisedItem.EstimatedTransportCost));
            Assert.False(acl.CanRead(M.SerialisedItem.ActualTransportCost));
            Assert.False(acl.CanWrite(M.SerialisedItem.ActualTransportCost));
            Assert.False(acl.CanRead(M.SerialisedItem.ExpectedRentalPriceDryLeaseLongTerm));
            Assert.False(acl.CanWrite(M.SerialisedItem.ExpectedRentalPriceDryLeaseLongTerm));
            Assert.False(acl.CanRead(M.SerialisedItem.ExpectedRentalPriceDryLeaseShortTerm));
            Assert.False(acl.CanWrite(M.SerialisedItem.ExpectedRentalPriceDryLeaseShortTerm));
            Assert.False(acl.CanRead(M.SerialisedItem.ExpectedRentalPriceFullServiceLongTerm));
            Assert.False(acl.CanWrite(M.SerialisedItem.ExpectedRentalPriceFullServiceLongTerm));
            Assert.False(acl.CanRead(M.SerialisedItem.ExpectedRentalPriceFullServiceShortTerm));
            Assert.False(acl.CanWrite(M.SerialisedItem.ExpectedRentalPriceFullServiceShortTerm));
            Assert.False(acl.CanRead(M.SerialisedItem.FromInitialImport));
            Assert.False(acl.CanWrite(M.SerialisedItem.FromInitialImport));

            Assert.True(acl.CanRead(M.SerialisedItem.SerialNumber));
            Assert.False(acl.CanWrite(M.SerialisedItem.SerialNumber));
            Assert.False(acl.CanExecute(M.SerialisedItem.Delete));

            Assert.True(acl.CanRead(M.SerialisedItem.PrimaryPhoto));
            Assert.True(acl.CanWrite(M.SerialisedItem.PrimaryPhoto));
            Assert.True(acl.CanRead(M.SerialisedItem.SecondaryPhotos));
            Assert.True(acl.CanWrite(M.SerialisedItem.SecondaryPhotos));
            Assert.True(acl.CanRead(M.SerialisedItem.AdditionalPhotos));
            Assert.True(acl.CanWrite(M.SerialisedItem.AdditionalPhotos));
            Assert.True(acl.CanRead(M.SerialisedItem.PrivatePhotos));
            Assert.True(acl.CanWrite(M.SerialisedItem.PrivatePhotos));
            Assert.True(acl.CanRead(M.SerialisedItem.PublicElectronicDocuments));
            Assert.True(acl.CanWrite(M.SerialisedItem.PublicElectronicDocuments));
            Assert.True(acl.CanRead(M.SerialisedItem.PublicLocalisedElectronicDocuments));
            Assert.True(acl.CanWrite(M.SerialisedItem.PublicLocalisedElectronicDocuments));
            Assert.True(acl.CanRead(M.SerialisedItem.PrivateElectronicDocuments));
            Assert.True(acl.CanWrite(M.SerialisedItem.PrivateElectronicDocuments));
            Assert.True(acl.CanRead(M.SerialisedItem.PrivateLocalisedElectronicDocuments));
            Assert.True(acl.CanWrite(M.SerialisedItem.PrivateLocalisedElectronicDocuments));
            Assert.True(acl.CanRead(M.SerialisedItem.SerialisedItemCharacteristics));
            Assert.True(acl.CanWrite(M.SerialisedItem.SerialisedItemCharacteristics));
        }

        [Fact]
        public void PurchaseShipmentItemSerialisedItemOwnInternalOrganisation()
        {
            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();

            var shipment = new PurchaseShipmentBuilder(this.Transaction)
                .WithShipToParty(this.InternalOrganisation)
                .Build();

            var shipmentItem = new ShipmentItemBuilder(this.Transaction)
                .WithSerialisedItem(serialisedItem)
                .Build();

            shipment.AddShipmentItem(shipmentItem);

            this.Transaction.Derive(false);

            this.Transaction.SetUser(this.Employee);

            Assert.True(serialisedItem.ExistSecurityTokens);

            var acl = new DatabaseAccessControl(this.Security, this.Employee)[serialisedItem];
            Assert.False(acl.CanRead(M.SerialisedItem.PurchasePrice));
            Assert.False(acl.CanWrite(M.SerialisedItem.PurchasePrice));
            Assert.False(acl.CanRead(M.SerialisedItem.AssignedPurchasePrice));
            Assert.False(acl.CanWrite(M.SerialisedItem.AssignedPurchasePrice));
            Assert.False(acl.CanRead(M.SerialisedItem.EstimatedRefurbishCost));
            Assert.False(acl.CanWrite(M.SerialisedItem.EstimatedRefurbishCost));
            Assert.False(acl.CanRead(M.SerialisedItem.ActualRefurbishCost));
            Assert.False(acl.CanWrite(M.SerialisedItem.ActualRefurbishCost));
            Assert.False(acl.CanRead(M.SerialisedItem.EstimatedTransportCost));
            Assert.False(acl.CanWrite(M.SerialisedItem.EstimatedTransportCost));
            Assert.False(acl.CanRead(M.SerialisedItem.ActualTransportCost));
            Assert.False(acl.CanWrite(M.SerialisedItem.ActualTransportCost));
            Assert.False(acl.CanRead(M.SerialisedItem.ExpectedRentalPriceDryLeaseLongTerm));
            Assert.False(acl.CanWrite(M.SerialisedItem.ExpectedRentalPriceDryLeaseLongTerm));
            Assert.False(acl.CanRead(M.SerialisedItem.ExpectedRentalPriceDryLeaseShortTerm));
            Assert.False(acl.CanWrite(M.SerialisedItem.ExpectedRentalPriceDryLeaseShortTerm));
            Assert.False(acl.CanRead(M.SerialisedItem.ExpectedRentalPriceFullServiceLongTerm));
            Assert.False(acl.CanWrite(M.SerialisedItem.ExpectedRentalPriceFullServiceLongTerm));
            Assert.False(acl.CanRead(M.SerialisedItem.ExpectedRentalPriceFullServiceShortTerm));
            Assert.False(acl.CanWrite(M.SerialisedItem.ExpectedRentalPriceFullServiceShortTerm));
            Assert.False(acl.CanRead(M.SerialisedItem.FromInitialImport));
            Assert.False(acl.CanWrite(M.SerialisedItem.FromInitialImport));

            Assert.True(acl.CanRead(M.SerialisedItem.SerialNumber));
            Assert.False(acl.CanWrite(M.SerialisedItem.SerialNumber));
            Assert.False(acl.CanExecute(M.SerialisedItem.Delete));

            Assert.True(acl.CanRead(M.SerialisedItem.PrimaryPhoto));
            Assert.True(acl.CanWrite(M.SerialisedItem.PrimaryPhoto));
            Assert.True(acl.CanRead(M.SerialisedItem.SecondaryPhotos));
            Assert.True(acl.CanWrite(M.SerialisedItem.SecondaryPhotos));
            Assert.True(acl.CanRead(M.SerialisedItem.AdditionalPhotos));
            Assert.True(acl.CanWrite(M.SerialisedItem.AdditionalPhotos));
            Assert.True(acl.CanRead(M.SerialisedItem.PrivatePhotos));
            Assert.True(acl.CanWrite(M.SerialisedItem.PrivatePhotos));
            Assert.True(acl.CanRead(M.SerialisedItem.PublicElectronicDocuments));
            Assert.True(acl.CanWrite(M.SerialisedItem.PublicElectronicDocuments));
            Assert.True(acl.CanRead(M.SerialisedItem.PublicLocalisedElectronicDocuments));
            Assert.True(acl.CanWrite(M.SerialisedItem.PublicLocalisedElectronicDocuments));
            Assert.True(acl.CanRead(M.SerialisedItem.PrivateElectronicDocuments));
            Assert.True(acl.CanWrite(M.SerialisedItem.PrivateElectronicDocuments));
            Assert.True(acl.CanRead(M.SerialisedItem.PrivateLocalisedElectronicDocuments));
            Assert.True(acl.CanWrite(M.SerialisedItem.PrivateLocalisedElectronicDocuments));
            Assert.True(acl.CanRead(M.SerialisedItem.SerialisedItemCharacteristics));
            Assert.True(acl.CanWrite(M.SerialisedItem.SerialisedItemCharacteristics));
        }

        [Fact]
        public void SerialisedItemBuyerOwnInternalOrganisation()
        {
            var serialisedItem = new SerialisedItemBuilder(this.Transaction).WithBuyer(this.InternalOrganisation).Build();

            this.Transaction.Derive(false);

            this.Transaction.SetUser(this.Employee);

            Assert.True(serialisedItem.ExistSecurityTokens);

            var acl = new DatabaseAccessControl(this.Security, this.Employee)[serialisedItem];
            Assert.False(acl.CanRead(M.SerialisedItem.PurchasePrice));
            Assert.False(acl.CanWrite(M.SerialisedItem.PurchasePrice));
            Assert.False(acl.CanRead(M.SerialisedItem.AssignedPurchasePrice));
            Assert.False(acl.CanWrite(M.SerialisedItem.AssignedPurchasePrice));
            Assert.False(acl.CanRead(M.SerialisedItem.EstimatedRefurbishCost));
            Assert.False(acl.CanWrite(M.SerialisedItem.EstimatedRefurbishCost));
            Assert.False(acl.CanRead(M.SerialisedItem.ActualRefurbishCost));
            Assert.False(acl.CanWrite(M.SerialisedItem.ActualRefurbishCost));
            Assert.False(acl.CanRead(M.SerialisedItem.EstimatedTransportCost));
            Assert.False(acl.CanWrite(M.SerialisedItem.EstimatedTransportCost));
            Assert.False(acl.CanRead(M.SerialisedItem.ActualTransportCost));
            Assert.False(acl.CanWrite(M.SerialisedItem.ActualTransportCost));
            Assert.False(acl.CanRead(M.SerialisedItem.ExpectedRentalPriceDryLeaseLongTerm));
            Assert.False(acl.CanWrite(M.SerialisedItem.ExpectedRentalPriceDryLeaseLongTerm));
            Assert.False(acl.CanRead(M.SerialisedItem.ExpectedRentalPriceDryLeaseShortTerm));
            Assert.False(acl.CanWrite(M.SerialisedItem.ExpectedRentalPriceDryLeaseShortTerm));
            Assert.False(acl.CanRead(M.SerialisedItem.ExpectedRentalPriceFullServiceLongTerm));
            Assert.False(acl.CanWrite(M.SerialisedItem.ExpectedRentalPriceFullServiceLongTerm));
            Assert.False(acl.CanRead(M.SerialisedItem.ExpectedRentalPriceFullServiceShortTerm));
            Assert.False(acl.CanWrite(M.SerialisedItem.ExpectedRentalPriceFullServiceShortTerm));
            Assert.False(acl.CanRead(M.SerialisedItem.FromInitialImport));
            Assert.False(acl.CanWrite(M.SerialisedItem.FromInitialImport));

            Assert.True(acl.CanRead(M.SerialisedItem.SerialNumber));
            Assert.False(acl.CanWrite(M.SerialisedItem.SerialNumber));
            Assert.False(acl.CanExecute(M.SerialisedItem.Delete));

            Assert.True(acl.CanRead(M.SerialisedItem.PrimaryPhoto));
            Assert.True(acl.CanWrite(M.SerialisedItem.PrimaryPhoto));
            Assert.True(acl.CanRead(M.SerialisedItem.SecondaryPhotos));
            Assert.True(acl.CanWrite(M.SerialisedItem.SecondaryPhotos));
            Assert.True(acl.CanRead(M.SerialisedItem.AdditionalPhotos));
            Assert.True(acl.CanWrite(M.SerialisedItem.AdditionalPhotos));
            Assert.True(acl.CanRead(M.SerialisedItem.PrivatePhotos));
            Assert.True(acl.CanWrite(M.SerialisedItem.PrivatePhotos));
            Assert.True(acl.CanRead(M.SerialisedItem.PublicElectronicDocuments));
            Assert.True(acl.CanWrite(M.SerialisedItem.PublicElectronicDocuments));
            Assert.True(acl.CanRead(M.SerialisedItem.PublicLocalisedElectronicDocuments));
            Assert.True(acl.CanWrite(M.SerialisedItem.PublicLocalisedElectronicDocuments));
            Assert.True(acl.CanRead(M.SerialisedItem.PrivateElectronicDocuments));
            Assert.True(acl.CanWrite(M.SerialisedItem.PrivateElectronicDocuments));
            Assert.True(acl.CanRead(M.SerialisedItem.PrivateLocalisedElectronicDocuments));
            Assert.True(acl.CanWrite(M.SerialisedItem.PrivateLocalisedElectronicDocuments));
            Assert.True(acl.CanRead(M.SerialisedItem.SerialisedItemCharacteristics));
            Assert.True(acl.CanWrite(M.SerialisedItem.SerialisedItemCharacteristics));
        }

        [Fact]
        public void SerialisedItemSellerOwnInternalOrganisation()
        {
            var serialisedItem = new SerialisedItemBuilder(this.Transaction).WithSeller(this.InternalOrganisation).Build();

            this.Transaction.Derive(false);

            this.Transaction.SetUser(this.Employee);

            Assert.True(serialisedItem.ExistSecurityTokens);

            var acl = new DatabaseAccessControl(this.Security, this.Employee)[serialisedItem];
            Assert.False(acl.CanRead(M.SerialisedItem.PurchasePrice));
            Assert.False(acl.CanWrite(M.SerialisedItem.PurchasePrice));
            Assert.False(acl.CanRead(M.SerialisedItem.AssignedPurchasePrice));
            Assert.False(acl.CanWrite(M.SerialisedItem.AssignedPurchasePrice));
            Assert.False(acl.CanRead(M.SerialisedItem.EstimatedRefurbishCost));
            Assert.False(acl.CanWrite(M.SerialisedItem.EstimatedRefurbishCost));
            Assert.False(acl.CanRead(M.SerialisedItem.ActualRefurbishCost));
            Assert.False(acl.CanWrite(M.SerialisedItem.ActualRefurbishCost));
            Assert.False(acl.CanRead(M.SerialisedItem.EstimatedTransportCost));
            Assert.False(acl.CanWrite(M.SerialisedItem.EstimatedTransportCost));
            Assert.False(acl.CanRead(M.SerialisedItem.ActualTransportCost));
            Assert.False(acl.CanWrite(M.SerialisedItem.ActualTransportCost));
            Assert.False(acl.CanRead(M.SerialisedItem.ExpectedRentalPriceDryLeaseLongTerm));
            Assert.False(acl.CanWrite(M.SerialisedItem.ExpectedRentalPriceDryLeaseLongTerm));
            Assert.False(acl.CanRead(M.SerialisedItem.ExpectedRentalPriceDryLeaseShortTerm));
            Assert.False(acl.CanWrite(M.SerialisedItem.ExpectedRentalPriceDryLeaseShortTerm));
            Assert.False(acl.CanRead(M.SerialisedItem.ExpectedRentalPriceFullServiceLongTerm));
            Assert.False(acl.CanWrite(M.SerialisedItem.ExpectedRentalPriceFullServiceLongTerm));
            Assert.False(acl.CanRead(M.SerialisedItem.ExpectedRentalPriceFullServiceShortTerm));
            Assert.False(acl.CanWrite(M.SerialisedItem.ExpectedRentalPriceFullServiceShortTerm));
            Assert.False(acl.CanRead(M.SerialisedItem.FromInitialImport));
            Assert.False(acl.CanWrite(M.SerialisedItem.FromInitialImport));

            Assert.True(acl.CanRead(M.SerialisedItem.SerialNumber));
            Assert.False(acl.CanWrite(M.SerialisedItem.SerialNumber));
            Assert.False(acl.CanExecute(M.SerialisedItem.Delete));

            Assert.True(acl.CanRead(M.SerialisedItem.PrimaryPhoto));
            Assert.True(acl.CanWrite(M.SerialisedItem.PrimaryPhoto));
            Assert.True(acl.CanRead(M.SerialisedItem.SecondaryPhotos));
            Assert.True(acl.CanWrite(M.SerialisedItem.SecondaryPhotos));
            Assert.True(acl.CanRead(M.SerialisedItem.AdditionalPhotos));
            Assert.True(acl.CanWrite(M.SerialisedItem.AdditionalPhotos));
            Assert.True(acl.CanRead(M.SerialisedItem.PrivatePhotos));
            Assert.True(acl.CanWrite(M.SerialisedItem.PrivatePhotos));
            Assert.True(acl.CanRead(M.SerialisedItem.PublicElectronicDocuments));
            Assert.True(acl.CanWrite(M.SerialisedItem.PublicElectronicDocuments));
            Assert.True(acl.CanRead(M.SerialisedItem.PublicLocalisedElectronicDocuments));
            Assert.True(acl.CanWrite(M.SerialisedItem.PublicLocalisedElectronicDocuments));
            Assert.True(acl.CanRead(M.SerialisedItem.PrivateElectronicDocuments));
            Assert.True(acl.CanWrite(M.SerialisedItem.PrivateElectronicDocuments));
            Assert.True(acl.CanRead(M.SerialisedItem.PrivateLocalisedElectronicDocuments));
            Assert.True(acl.CanWrite(M.SerialisedItem.PrivateLocalisedElectronicDocuments));
            Assert.True(acl.CanRead(M.SerialisedItem.SerialisedItemCharacteristics));
            Assert.True(acl.CanWrite(M.SerialisedItem.SerialisedItemCharacteristics));
        }

        [Fact]
        public void SerialisedItemOwnedByOwnInternalOrganisation()
        {
            var serialisedItem = new SerialisedItemBuilder(this.Transaction).WithOwnedBy(this.InternalOrganisation).Build();

            this.Transaction.Derive(false);

            this.Transaction.SetUser(this.Employee);

            Assert.True(serialisedItem.ExistSecurityTokens);

            var acl = new DatabaseAccessControl(this.Security, this.Employee)[serialisedItem];
            Assert.False(acl.CanRead(M.SerialisedItem.PurchasePrice));
            Assert.False(acl.CanWrite(M.SerialisedItem.PurchasePrice));
            Assert.False(acl.CanRead(M.SerialisedItem.AssignedPurchasePrice));
            Assert.False(acl.CanWrite(M.SerialisedItem.AssignedPurchasePrice));
            Assert.False(acl.CanRead(M.SerialisedItem.EstimatedRefurbishCost));
            Assert.False(acl.CanWrite(M.SerialisedItem.EstimatedRefurbishCost));
            Assert.False(acl.CanRead(M.SerialisedItem.ActualRefurbishCost));
            Assert.False(acl.CanWrite(M.SerialisedItem.ActualRefurbishCost));
            Assert.False(acl.CanRead(M.SerialisedItem.EstimatedTransportCost));
            Assert.False(acl.CanWrite(M.SerialisedItem.EstimatedTransportCost));
            Assert.False(acl.CanRead(M.SerialisedItem.ActualTransportCost));
            Assert.False(acl.CanWrite(M.SerialisedItem.ActualTransportCost));
            Assert.False(acl.CanRead(M.SerialisedItem.ExpectedRentalPriceDryLeaseLongTerm));
            Assert.False(acl.CanWrite(M.SerialisedItem.ExpectedRentalPriceDryLeaseLongTerm));
            Assert.False(acl.CanRead(M.SerialisedItem.ExpectedRentalPriceDryLeaseShortTerm));
            Assert.False(acl.CanWrite(M.SerialisedItem.ExpectedRentalPriceDryLeaseShortTerm));
            Assert.False(acl.CanRead(M.SerialisedItem.ExpectedRentalPriceFullServiceLongTerm));
            Assert.False(acl.CanWrite(M.SerialisedItem.ExpectedRentalPriceFullServiceLongTerm));
            Assert.False(acl.CanRead(M.SerialisedItem.ExpectedRentalPriceFullServiceShortTerm));
            Assert.False(acl.CanWrite(M.SerialisedItem.ExpectedRentalPriceFullServiceShortTerm));
            Assert.False(acl.CanRead(M.SerialisedItem.FromInitialImport));
            Assert.False(acl.CanWrite(M.SerialisedItem.FromInitialImport));

            Assert.True(acl.CanRead(M.SerialisedItem.SerialNumber));
            Assert.False(acl.CanWrite(M.SerialisedItem.SerialNumber));
            Assert.False(acl.CanExecute(M.SerialisedItem.Delete));

            Assert.True(acl.CanRead(M.SerialisedItem.PrimaryPhoto));
            Assert.True(acl.CanWrite(M.SerialisedItem.PrimaryPhoto));
            Assert.True(acl.CanRead(M.SerialisedItem.SecondaryPhotos));
            Assert.True(acl.CanWrite(M.SerialisedItem.SecondaryPhotos));
            Assert.True(acl.CanRead(M.SerialisedItem.AdditionalPhotos));
            Assert.True(acl.CanWrite(M.SerialisedItem.AdditionalPhotos));
            Assert.True(acl.CanRead(M.SerialisedItem.PrivatePhotos));
            Assert.True(acl.CanWrite(M.SerialisedItem.PrivatePhotos));
            Assert.True(acl.CanRead(M.SerialisedItem.PublicElectronicDocuments));
            Assert.True(acl.CanWrite(M.SerialisedItem.PublicElectronicDocuments));
            Assert.True(acl.CanRead(M.SerialisedItem.PublicLocalisedElectronicDocuments));
            Assert.True(acl.CanWrite(M.SerialisedItem.PublicLocalisedElectronicDocuments));
            Assert.True(acl.CanRead(M.SerialisedItem.PrivateElectronicDocuments));
            Assert.True(acl.CanWrite(M.SerialisedItem.PrivateElectronicDocuments));
            Assert.True(acl.CanRead(M.SerialisedItem.PrivateLocalisedElectronicDocuments));
            Assert.True(acl.CanWrite(M.SerialisedItem.PrivateLocalisedElectronicDocuments));
            Assert.True(acl.CanRead(M.SerialisedItem.SerialisedItemCharacteristics));
            Assert.True(acl.CanWrite(M.SerialisedItem.SerialisedItemCharacteristics));
        }

        [Fact]
        public void SerialisedItemRentedByOwnInternalOrganisation()
        {
            var serialisedItem = new SerialisedItemBuilder(this.Transaction).WithRentedBy(this.InternalOrganisation).Build();

            this.Transaction.Derive(false);

            this.Transaction.SetUser(this.Employee);

            Assert.True(serialisedItem.ExistSecurityTokens);

            var acl = new DatabaseAccessControl(this.Security, this.Employee)[serialisedItem];
            Assert.False(acl.CanRead(M.SerialisedItem.PurchasePrice));
            Assert.False(acl.CanWrite(M.SerialisedItem.PurchasePrice));
            Assert.False(acl.CanRead(M.SerialisedItem.AssignedPurchasePrice));
            Assert.False(acl.CanWrite(M.SerialisedItem.AssignedPurchasePrice));
            Assert.False(acl.CanRead(M.SerialisedItem.EstimatedRefurbishCost));
            Assert.False(acl.CanWrite(M.SerialisedItem.EstimatedRefurbishCost));
            Assert.False(acl.CanRead(M.SerialisedItem.ActualRefurbishCost));
            Assert.False(acl.CanWrite(M.SerialisedItem.ActualRefurbishCost));
            Assert.False(acl.CanRead(M.SerialisedItem.EstimatedTransportCost));
            Assert.False(acl.CanWrite(M.SerialisedItem.EstimatedTransportCost));
            Assert.False(acl.CanRead(M.SerialisedItem.ActualTransportCost));
            Assert.False(acl.CanWrite(M.SerialisedItem.ActualTransportCost));
            Assert.False(acl.CanRead(M.SerialisedItem.ExpectedRentalPriceDryLeaseLongTerm));
            Assert.False(acl.CanWrite(M.SerialisedItem.ExpectedRentalPriceDryLeaseLongTerm));
            Assert.False(acl.CanRead(M.SerialisedItem.ExpectedRentalPriceDryLeaseShortTerm));
            Assert.False(acl.CanWrite(M.SerialisedItem.ExpectedRentalPriceDryLeaseShortTerm));
            Assert.False(acl.CanRead(M.SerialisedItem.ExpectedRentalPriceFullServiceLongTerm));
            Assert.False(acl.CanWrite(M.SerialisedItem.ExpectedRentalPriceFullServiceLongTerm));
            Assert.False(acl.CanRead(M.SerialisedItem.ExpectedRentalPriceFullServiceShortTerm));
            Assert.False(acl.CanWrite(M.SerialisedItem.ExpectedRentalPriceFullServiceShortTerm));
            Assert.False(acl.CanRead(M.SerialisedItem.FromInitialImport));
            Assert.False(acl.CanWrite(M.SerialisedItem.FromInitialImport));

            Assert.True(acl.CanRead(M.SerialisedItem.SerialNumber));
            Assert.False(acl.CanWrite(M.SerialisedItem.SerialNumber));
            Assert.False(acl.CanExecute(M.SerialisedItem.Delete));

            Assert.True(acl.CanRead(M.SerialisedItem.PrimaryPhoto));
            Assert.True(acl.CanWrite(M.SerialisedItem.PrimaryPhoto));
            Assert.True(acl.CanRead(M.SerialisedItem.SecondaryPhotos));
            Assert.True(acl.CanWrite(M.SerialisedItem.SecondaryPhotos));
            Assert.True(acl.CanRead(M.SerialisedItem.AdditionalPhotos));
            Assert.True(acl.CanWrite(M.SerialisedItem.AdditionalPhotos));
            Assert.True(acl.CanRead(M.SerialisedItem.PrivatePhotos));
            Assert.True(acl.CanWrite(M.SerialisedItem.PrivatePhotos));
            Assert.True(acl.CanRead(M.SerialisedItem.PublicElectronicDocuments));
            Assert.True(acl.CanWrite(M.SerialisedItem.PublicElectronicDocuments));
            Assert.True(acl.CanRead(M.SerialisedItem.PublicLocalisedElectronicDocuments));
            Assert.True(acl.CanWrite(M.SerialisedItem.PublicLocalisedElectronicDocuments));
            Assert.True(acl.CanRead(M.SerialisedItem.PrivateElectronicDocuments));
            Assert.True(acl.CanWrite(M.SerialisedItem.PrivateElectronicDocuments));
            Assert.True(acl.CanRead(M.SerialisedItem.PrivateLocalisedElectronicDocuments));
            Assert.True(acl.CanWrite(M.SerialisedItem.PrivateLocalisedElectronicDocuments));
            Assert.True(acl.CanRead(M.SerialisedItem.SerialisedItemCharacteristics));
            Assert.True(acl.CanWrite(M.SerialisedItem.SerialisedItemCharacteristics));
        }
    }
}
