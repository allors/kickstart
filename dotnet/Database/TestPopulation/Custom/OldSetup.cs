// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Setup.cs" company="Allors bvba">
//   Copyright 2002-2012 Allors bvba.
//
// Dual Licensed under
//   a) the General Public Licence v3 (GPL)
//   b) the Allors License
//
// The GPL License is included in the file gpl.txt.
// The Allors License is an addendum to your contract.
//
// Allors Applications is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// For more information visit http://www.allors.com/legal
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System.Linq;
using Allors.Database;
using Allors.Database.Domain;
using Allors.Database.Domain.TestPopulation;
using Allors.Database.Meta;
using Organisation = Allors.Database.Domain.Organisation;

namespace Allors
{
    public partial class OldSetup
    {
        private readonly ITransaction transaction;

        public OldSetup(ITransaction transaction)
        {
            this.transaction = transaction;
        }

        public void Demo()
        {
            var m = this.transaction.Database.Services.Get<MetaPopulation>();

            var administrator = new PersonBuilder(transaction)
                .WithFirstName("Jane")
                .WithLastName("Doe")
                .WithUserName("administrator")
                .Build();

            new UserGroups(transaction).Administrators.AddMember(administrator);

            transaction.Services.Get<IUserService>().User = administrator;

            // Give Administrator access
            foreach (var @this in new Organisations(this.transaction).Extent().Where(v => v.IsInternalOrganisation))
            {
                new EmploymentBuilder(this.transaction).WithEmployee(administrator).WithEmployer(@this).Build();
                @this.AddProductQuoteApprover(administrator);
                @this.AddPurchaseOrderApproversLevel1(administrator);
                @this.AddPurchaseOrderApproversLevel2(administrator);
                @this.AddBlueCollarWorker(administrator);
            }

            var internalOrganisation = new Organisations(this.transaction).Extent().First(v => v.IsInternalOrganisation);

            var customerSalesAgreement = new SalesAgreementBuilder(this.transaction).WithDescription("default payment terms")
                .WithAgreementTerm(
                    new InvoiceTermBuilder(this.transaction).WithTermType(new InvoiceTermTypes(this.transaction).PaymentNetDays)
                        .WithTermValue("30").Build()).Build();

            var customer = new OrganisationBuilder(this.transaction)
                .WithName("a customer")
                .WithTaxNumber("cust.tax number").Build();

            new CustomerRelationshipBuilder(this.transaction)
                .WithCustomer(customer)
                .WithInternalOrganisation(internalOrganisation)
                .WithAgreement(customerSalesAgreement)
                .WithFromDate(this.transaction.Now().AddDays(-1)).Build();

            var contactMechanism = new PostalAddressBuilder(this.transaction)
                .WithAddress1("Haverwerf 15")
                .WithLocality("Mechelen")
                .WithPostalCode("2800")
                .WithCountry(new Countries(this.transaction).FindBy(m.Country.IsoCode, "BE"))
                .Build();

            var partyContactMechanism = new PartyContactMechanismBuilder(this.transaction)
                .WithParty(customer)
                .WithUseAsDefault(true)
                .WithContactMechanism(contactMechanism)
                .WithContactPurpose(new ContactMechanismPurposes(this.transaction).GeneralCorrespondence).Build();

            var serialisedGood = new UnifiedGoodBuilder(this.transaction)
                .WithName("serialised good")
                .WithInventoryItemKind(new InventoryItemKinds(this.transaction).Serialised)
                .WithVatRegime(new VatRegimes(this.transaction).ZeroRated)
                .Build();

            var serialisedItem = new SerialisedItemBuilder(this.transaction)
                .WithSerialNumber("123")
                .WithOwnedBy(internalOrganisation)
                .WithOwnership(new Ownerships(this.transaction).Own)
                .WithSerialisedItemAvailability(new SerialisedItemAvailabilities(this.transaction).Available)
                .WithExpectedRentalPriceDryLeaseLongTerm(100M)
                .WithExpectedRentalPriceFullServiceLongTerm(500M)
                .Build();
            serialisedGood.AddSerialisedItem(serialisedItem);

            this.transaction.Derive();

            new SerialisedInventoryItemBuilder(this.transaction).WithPart(serialisedGood).WithSerialisedItem(serialisedItem).Build();

            var nonSerialisedgood = new UnifiedGoodBuilder(this.transaction)
                .WithName("good")
                .WithInventoryItemKind(new InventoryItemKinds(this.transaction).NonSerialised)
                .WithVatRegime(new VatRegimes(this.transaction).ZeroRated)
                .Build();

            var productCategory = new ProductCategoryBuilder(this.transaction)
                .WithCatScope(new Scopes(this.transaction).Public)
                .WithInternalOrganisation(internalOrganisation)
                .WithProduct(serialisedGood)
                .WithProduct(nonSerialisedgood)
                .WithName("products").Build();

            this.transaction.Derive();

            new InventoryItemTransactionBuilder(this.transaction).WithPart(nonSerialisedgood).WithQuantity(12)
                .WithReason(new InventoryTransactionReasons(this.transaction).IncomingShipment).Build();

            var item1 = new SalesInvoiceItemBuilder(this.transaction)
                .WithDescription("first item")
                .WithProduct(serialisedGood).WithAssignedUnitPrice(3000).WithQuantity(1).WithMessage(
                    @"line1
                line2")
                .WithInvoiceItemType(new InvoiceItemTypes(this.transaction).ProductItem).Build();

            var item2 = new SalesInvoiceItemBuilder(this.transaction)
                .WithDescription("second item")
                .WithProduct(nonSerialisedgood)
                .WithAssignedUnitPrice(2000)
                .WithQuantity(2)
                .WithInvoiceItemType(new InvoiceItemTypes(this.transaction).ProductItem).Build();

            var item3 = new SalesInvoiceItemBuilder(this.transaction)
                .WithDescription("Fee")
                .WithAssignedUnitPrice(100)
                .WithQuantity(1)
                .WithInvoiceItemType(new InvoiceItemTypes(this.transaction).Service).Build();

            var invoice = new SalesInvoiceBuilder(this.transaction)
                .WithBilledFrom(internalOrganisation)
                .WithBillToCustomer(customer)
                .WithAssignedBillToContactMechanism(contactMechanism)
                .WithSalesInvoiceItem(item1)
                .WithSalesInvoiceItem(item2)
                .WithSalesInvoiceItem(item3)
                .WithCustomerReference("a reference number")
                .WithDescription("Sale of 1 used Aircraft Towbar")
                .WithSalesInvoiceType(new SalesInvoiceTypes(this.transaction).SalesInvoice)
                .WithAssignedVatRegime(new VatRegimes(this.transaction).BelgiumStandard)
                .Build();

            this.transaction.Derive();

            var sparePart = new NonUnifiedPartBuilder(this.transaction).WithNonSerialisedDefaults(internalOrganisation).Build();

            this.transaction.Derive();

            new InventoryItemTransactionBuilder(this.transaction)
                .WithPart(sparePart)
                .WithQuantity(10)
                .WithReason(new InventoryTransactionReasons(this.transaction).PhysicalCount)
                .Build();

            this.transaction.Derive();

            var workTask = new WorkTaskBuilder(this.transaction).WithName("Do something").WithCustomer(internalOrganisation).WithTakenBy(internalOrganisation).Build();
            new WorkEffortFixedAssetAssignmentBuilder(this.transaction).WithAssignment(workTask).WithFixedAsset(serialisedItem).Build();

            #region request/quote/order

            var request = new RequestForQuoteBuilder(this.transaction)
                .WithRecipient(internalOrganisation)
                .WithEmailAddress("meknip@xs4all.nl")
                .WithTelephoneCountryCode("+31")
                .WithTelephoneNumber("0613568160")
                .WithDescription("anonymous request").Build();

            var requestItem = new RequestItemBuilder(this.transaction)
                .WithProduct(new Goods(this.transaction).Extent().First)
                .WithQuantity(1).Build();

            request.AddRequestItem(requestItem);

            var quoteForSale = new ProductQuoteBuilder(this.transaction)
                .WithIssuer(internalOrganisation)
                .WithDescription("quote")
                .WithReceiver(customer)
                .WithFullfillContactMechanism(customer.GeneralCorrespondence).Build();

            var quoteItemForSale = new QuoteItemBuilder(this.transaction)
                .WithProduct(serialisedGood)
                .WithSerialisedItem(serialisedItem)
                .WithSaleKind(new SaleKinds(this.transaction).Sale)
                .WithQuantity(1)
                .WithAssignedUnitPrice(10).Build();

            quoteForSale.AddQuoteItem(quoteItemForSale);

            var quoteForRental = new ProductQuoteBuilder(this.transaction)
                .WithIssuer(internalOrganisation)
                .WithDescription("quote")
                .WithReceiver(customer)
                .WithFullfillContactMechanism(customer.GeneralCorrespondence).Build();

            var quoteItemForRental = new QuoteItemBuilder(this.transaction)
                .WithInvoiceItemType(new InvoiceItemTypes(this.transaction).ProductItem)
                .WithRentalType(new RentalTypes(this.transaction).Extent().First())
                .WithProduct(serialisedGood)
                .WithSaleKind(new SaleKinds(this.transaction).Rental)
                .WithSerialisedItem(serialisedItem)
                .WithQuantity(1)
                .WithAssignedUnitPrice(serialisedItem.ExpectedRentalPriceFullServiceLongTerm).Build();

            quoteForRental.AddQuoteItem(quoteItemForRental);

            var salesOrder = new SalesOrderBuilder(this.transaction).WithTakenBy(internalOrganisation).WithShipToCustomer(customer)
                .Build();

            var salesOrderItem = new SalesOrderItemBuilder(this.transaction).WithProduct(nonSerialisedgood).WithQuantityOrdered(1).WithSaleKind(new SaleKinds(this.transaction).Sale).WithAssignedUnitPrice(10).Build();

            salesOrder.AddSalesOrderItem(salesOrderItem);

            #endregion request/quote/order

            this.transaction.Derive();
            this.transaction.Commit();
        }

        public void End2End()
        {
            var administrator = new PersonBuilder(transaction)
                .WithFirstName("Jane")
                .WithLastName("Doe")
                .WithUserName("administrator")
                .Build();

            new UserGroups(transaction).Administrators.AddMember(administrator);

            transaction.Services.Get<IUserService>().User = administrator;

            var internalOrganisation = new Organisations(this.transaction).Extent().First(v => v.IsInternalOrganisation);
            var defaultFacility = internalOrganisation.StoresWhereInternalOrganisation.First(v => v.ExistDefaultFacility)?.DefaultFacility;

            var b2BCustomer = newB2BCustomer(internalOrganisation);

            var serialisedItem = new SerialisedItemBuilder(this.transaction)
                .WithSerialNumber("123")
                .WithOwnedBy(internalOrganisation)
                .WithOwnership(new Ownerships(this.transaction).Own)
                .Build();

            var serialisedGood = new UnifiedGoodBuilder(this.transaction)
                .WithName("serialised good")
                .WithDefaultFacility(defaultFacility)
                .WithInventoryItemKind(new InventoryItemKinds(this.transaction).Serialised)
                .WithVatRegime(new VatRegimes(this.transaction).ZeroRated)
                .WithSerialisedItem(serialisedItem)
                .WithUnitOfMeasure(new UnitsOfMeasure(this.transaction).Kilogram)
                .Build();

            var good = new UnifiedGoodBuilder(this.transaction)
                .WithName("good")
                .WithInventoryItemKind(new InventoryItemKinds(this.transaction).NonSerialised)
                .WithVatRegime(new VatRegimes(this.transaction).ZeroRated)
                .Build();

            this.transaction.Derive();

            new SerialisedInventoryItemBuilder(this.transaction)
                .WithSerialisedItem(serialisedItem)
                .WithPart(serialisedGood)
                .WithFacility(defaultFacility)
                .WithUnitOfMeasure(new UnitsOfMeasure(this.transaction).Kilogram)
                .Build();

            new InventoryItemTransactionBuilder(this.transaction)
                .WithSerialisedItem(serialisedItem)
                .WithPart(serialisedGood)
                .WithQuantity(1)
                .WithReason(new InventoryTransactionReasons(this.transaction).IncomingShipment)
                .Build();

            var item1 = new SalesInvoiceItemBuilder(this.transaction)
                .WithDescription("first item")
                .WithProduct(serialisedGood)
                .WithAssignedUnitPrice(3000)
                .WithQuantity(1)
                .WithMessage(@"line1
                line2")
                .WithInvoiceItemType(new InvoiceItemTypes(this.transaction).ProductItem)
                .Build();

            var item2 = new SalesInvoiceItemBuilder(this.transaction)
                .WithDescription("second item")
                .WithProduct(good)
                .WithAssignedUnitPrice(2000)
                .WithQuantity(2)
                .WithInvoiceItemType(new InvoiceItemTypes(this.transaction).ProductItem)
                .Build();

            var item3 = new SalesInvoiceItemBuilder(this.transaction)
                .WithDescription("Service")
                .WithAssignedUnitPrice(100)
                .WithQuantity(1)
                .WithInvoiceItemType(new InvoiceItemTypes(this.transaction).Service)
                .Build();

            var invoice = new SalesInvoiceBuilder(this.transaction)
                .WithBilledFrom(internalOrganisation)
                .WithBillToCustomer(b2BCustomer)
                .WithAssignedBillToContactMechanism(b2BCustomer.BillingAddress)
                .WithSalesInvoiceItem(item1)
                .WithSalesInvoiceItem(item2)
                .WithSalesInvoiceItem(item3)
                .WithCustomerReference("a reference number")
                .WithDescription("Sale of 1 used Aircraft Towbar")
                .WithSalesInvoiceType(new SalesInvoiceTypes(this.transaction).SalesInvoice)
                .WithAssignedVatRegime(new VatRegimes(this.transaction).BelgiumStandard)
                .Build();

            this.transaction.Derive();

            #region request/quote/order

            var request = new RequestForQuoteBuilder(this.transaction)
                .WithRecipient(internalOrganisation)
                .WithEmailAddress("meknip@xs4all.nl")
                .WithTelephoneCountryCode("+31")
                .WithTelephoneNumber("0613568160")
                .WithDescription("anonymous request")
                .Build();

            var requestItem = new RequestItemBuilder(this.transaction)
                .WithProduct(serialisedGood)
                .WithSerialisedItem(serialisedItem)
                .WithQuantity(1)
                .Build();

            request.AddRequestItem(requestItem);

            var quote = new ProductQuoteBuilder(this.transaction)
                .WithIssuer(internalOrganisation)
                .WithDescription("quote")
                .WithReceiver(b2BCustomer)
                .WithFullfillContactMechanism(b2BCustomer.GeneralCorrespondence)
                .Build();

            var quoteItem = new QuoteItemBuilder(this.transaction)
                .WithProduct(serialisedGood)
                .WithSerialisedItem(serialisedItem)
                .WithSaleKind(new SaleKinds(this.transaction).Sale)
                .WithQuantity(1)
                .WithAssignedUnitPrice(10)
                .Build();

            quote.AddQuoteItem(quoteItem);

            var salesOrder = new SalesOrderBuilder(this.transaction)
                .WithTakenBy(internalOrganisation)
                .WithShipToCustomer(b2BCustomer)
                .Build();

            var salesOrderItem = new SalesOrderItemBuilder(this.transaction)
                .WithProduct(good)
                .WithSaleKind(new SaleKinds(this.transaction).Sale)
                .WithQuantityOrdered(1)
                .WithAssignedUnitPrice(10)
                .Build();

            salesOrder.AddSalesOrderItem(salesOrderItem);

            #endregion request/quote/order

            this.transaction.Derive();
            this.transaction.Commit();
        }

        private Organisation newB2BCustomer(Organisation internalOrganisation)
        {
            var customer = new OrganisationBuilder(this.transaction).WithDefaults().Build();

            new PartyContactMechanismBuilder(this.transaction)
                .WithParty(customer)
                .WithUseAsDefault(true)
                .WithContactMechanism(new PostalAddressBuilder(this.transaction).WithDefaults().Build())
                .WithContactPurpose(new ContactMechanismPurposes(this.transaction).GeneralCorrespondence)
                .WithContactPurpose(new ContactMechanismPurposes(this.transaction).ShippingAddress)
                .WithContactPurpose(new ContactMechanismPurposes(this.transaction).HeadQuarters)
                .Build();

            new PartyContactMechanismBuilder(this.transaction)
                .WithParty(customer)
                .WithUseAsDefault(true)
                .WithContactMechanism(new EmailAddressBuilder(this.transaction).WithDefaults().Build())
                .WithContactPurpose(new ContactMechanismPurposes(this.transaction).GeneralEmail)
                .WithContactPurpose(new ContactMechanismPurposes(this.transaction).BillingAddress)
                .Build();

            new PartyContactMechanismBuilder(this.transaction)
                .WithParty(customer)
                .WithUseAsDefault(true)
                .WithContactMechanism(new WebAddressBuilder(this.transaction).WithDefaults().Build())
                .WithContactPurpose(new ContactMechanismPurposes(this.transaction).InternetAddress)
                .Build();

            new PartyContactMechanismBuilder(this.transaction)
                .WithParty(customer)
                .WithUseAsDefault(true)
                .WithContactMechanism(new TelecommunicationsNumberBuilder(this.transaction).WithDefaults().Build())
                .WithContactPurpose(new ContactMechanismPurposes(this.transaction).GeneralPhoneNumber)
                .WithContactPurpose(new ContactMechanismPurposes(this.transaction).BillingInquiriesPhone)
                .Build();

            new PartyContactMechanismBuilder(this.transaction)
                .WithParty(customer)
                .WithUseAsDefault(true)
                .WithContactMechanism(new TelecommunicationsNumberBuilder(this.transaction).WithDefaults().Build())
                .WithContactPurpose(new ContactMechanismPurposes(this.transaction).OrderInquiriesPhone)
                .WithContactPurpose(new ContactMechanismPurposes(this.transaction).ShippingInquiriesPhone)
                .Build();

            new CustomerRelationshipBuilder(this.transaction).WithCustomer(customer).WithInternalOrganisation(internalOrganisation).WithFromDate(this.transaction.Now().AddDays(-1)).Build();

            new OrganisationContactRelationshipBuilder(transaction)
                .WithContact(new PersonBuilder(transaction).WithDefaults().Build())
                .WithOrganisation(customer)
                .WithFromDate(transaction.Now().AddDays(-1))
                .Build();

            return customer;
        }
    }
}
