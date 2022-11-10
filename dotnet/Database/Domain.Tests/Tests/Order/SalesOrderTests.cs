// <copyright file="SalesOrderTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using Allors.Database.Domain.TestPopulation;
    using System.Linq;
    using Xunit;

    public class SalesOrderTests : DomainTest, IClassFixture<Fixture>
    {
        public SalesOrderTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void GivenSalesOrderTakenByBelgianInternalOrganisationForRentingGoodsToBusinessCustomer_WhenDerived_ThenVatClauseIsSet()
        {
            var belgianInternalOrganisation = new OrganisationBuilder(this.Transaction)
                .WithIsInternalOrganisation(true)
                .WithName("Belgian InternalOrganisation")
                .Build();

            var takenByAddress = new PostalAddressBuilder(this.Transaction)
                .WithAddress1("address")
                .WithLocality("city")
                .WithCountry(new Countries(this.Transaction).CountryByIsoCode["BE"])
                .Build();

            var takenByContactMechanism = new PartyContactMechanismBuilder(this.Transaction)
                .WithParty(belgianInternalOrganisation)
                .WithContactMechanism(takenByAddress)
                .WithContactPurpose(new ContactMechanismPurposes(this.Transaction).RegisteredOffice)
                .WithContactPurpose(new ContactMechanismPurposes(this.Transaction).BillingAddress)
                .WithUseAsDefault(true)
                .Build();

            var shipFromAddress = new PostalAddressBuilder(this.Transaction)
                .WithAddress1("address")
                .WithLocality("city")
                .WithCountry(new Countries(this.Transaction).CountryByIsoCode["BE"])
                .Build();

            var shipFromContactMechanism = new PartyContactMechanismBuilder(this.Transaction)
                .WithParty(belgianInternalOrganisation)
                .WithContactMechanism(shipFromAddress)
                .WithContactPurpose(new ContactMechanismPurposes(this.Transaction).ShippingAddress)
                .WithUseAsDefault(true)
                .Build();

            new StoreBuilder(this.Transaction)
                .WithName("store")
                .WithBillingProcess(new BillingProcesses(this.Transaction).BillingForShipmentItems)
                .WithInternalOrganisation(belgianInternalOrganisation)
                .WithDefaultShipmentMethod(new ShipmentMethods(this.Transaction).Ground)
                .WithDefaultCarrier(new Carriers(this.Transaction).Fedex)
                .WithDefaultCollectionMethod(new PaymentMethods(this.Transaction).Extent().First())
                .WithIsImmediatelyPacked(true)
                .Build();

            var shipToAddress = new PostalAddressBuilder(this.Transaction)
                .WithAddress1("address")
                .WithLocality("city")
                .WithCountry(new Countries(this.Transaction).CountryByIsoCode["US"])
                .Build();

            var customer = new OrganisationBuilder(this.Transaction).WithName("customer").Build();
            new PartyContactMechanismBuilder(this.Transaction)
                .WithParty(customer)
                .WithContactMechanism(shipToAddress)
                .WithContactPurpose(new ContactMechanismPurposes(this.Transaction).ShippingAddress)
                .WithUseAsDefault(true)
                .Build();
            new CustomerRelationshipBuilder(this.Transaction).WithFromDate(this.Transaction.Now()).WithCustomer(customer).WithInternalOrganisation(belgianInternalOrganisation).Build();

            this.Transaction.Derive();

            // seller is belgian company, renting good to customer
            var order = new SalesOrderBuilder(this.Transaction)
                .WithTakenBy(belgianInternalOrganisation)
                .WithBillToCustomer(customer)
                .WithShipToCustomer(customer)
                .WithAssignedShipToAddress(shipToAddress)
                .WithAssignedVatRegime(new VatRegimes(this.Transaction).ServiceB2B)
                .Build();

            this.Transaction.Derive();

            Assert.Equal(new VatClauses(this.Transaction).ServiceB2B, order.DerivedVatClause);
        }

        [Fact]
        public void GivenSalesOrderTakenByBelgianInternalOrganisationForSellingToInsideEUBusinessCustomer_WhenDerived_ThenVatClauseIsSet()
        {
            var belgianInternalOrganisation = new OrganisationBuilder(this.Transaction)
                .WithIsInternalOrganisation(true)
                .WithName("Belgian InternalOrganisation")
                .Build();

            var takenByAddress = new PostalAddressBuilder(this.Transaction)
                .WithAddress1("address")
                .WithLocality("city")
                .WithCountry(new Countries(this.Transaction).CountryByIsoCode["BE"])
                .Build();

            var takenByContactMechanism = new PartyContactMechanismBuilder(this.Transaction)
                .WithParty(belgianInternalOrganisation)
                .WithContactMechanism(takenByAddress)
                .WithContactPurpose(new ContactMechanismPurposes(this.Transaction).RegisteredOffice)
                .WithContactPurpose(new ContactMechanismPurposes(this.Transaction).BillingAddress)
                .WithUseAsDefault(true)
                .Build();

            var shipFromAddress = new PostalAddressBuilder(this.Transaction)
                .WithAddress1("address")
                .WithLocality("city")
                .WithCountry(new Countries(this.Transaction).CountryByIsoCode["BE"])
                .Build();

            var shipFromContactMechanism = new PartyContactMechanismBuilder(this.Transaction)
                .WithParty(belgianInternalOrganisation)
                .WithContactMechanism(shipFromAddress)
                .WithContactPurpose(new ContactMechanismPurposes(this.Transaction).ShippingAddress)
                .WithUseAsDefault(true)
                .Build();

            new StoreBuilder(this.Transaction)
                .WithName("store")
                .WithBillingProcess(new BillingProcesses(this.Transaction).BillingForShipmentItems)
                .WithInternalOrganisation(belgianInternalOrganisation)
                .WithDefaultShipmentMethod(new ShipmentMethods(this.Transaction).Ground)
                .WithDefaultCarrier(new Carriers(this.Transaction).Fedex)
                .WithDefaultCollectionMethod(new PaymentMethods(this.Transaction).Extent().First())
                .WithIsImmediatelyPacked(true)
                .Build();

            var shipToAddress = new PostalAddressBuilder(this.Transaction)
                .WithAddress1("address")
                .WithLocality("city")
                .WithCountry(new Countries(this.Transaction).CountryByIsoCode["NL"])
                .Build();

            var customer = new OrganisationBuilder(this.Transaction).WithName("customer").Build();
            new PartyContactMechanismBuilder(this.Transaction)
                .WithParty(customer)
                .WithContactMechanism(shipToAddress)
                .WithContactPurpose(new ContactMechanismPurposes(this.Transaction).ShippingAddress)
                .WithUseAsDefault(true)
                .Build();
            new CustomerRelationshipBuilder(this.Transaction).WithFromDate(this.Transaction.Now()).WithCustomer(customer).WithInternalOrganisation(belgianInternalOrganisation).Build();

            this.Transaction.Derive();

            // seller is belgian company, selling to EU customer, shipping From Belgium inside EU
            var order = new SalesOrderBuilder(this.Transaction)
                .WithTakenBy(belgianInternalOrganisation)
                .WithBillToCustomer(customer)
                .WithShipToCustomer(customer)
                .WithAssignedShipToAddress(shipToAddress)
                .WithAssignedVatRegime(new VatRegimes(this.Transaction).IntraCommunautair)
                .Build();

            this.Transaction.Derive();

            Assert.Equal(new VatClauses(this.Transaction).IntraCommunautair, order.DerivedVatClause);
        }

        [Fact]
        public void GivenSalesOrderTakenByBelgianInternalOrganisationForSellingToOutsideEUBusinessCustomer_WhenDerived_ThenVatClauseIsSet()
        {
            var belgianInternalOrganisation = new OrganisationBuilder(this.Transaction)
                .WithIsInternalOrganisation(true)
                .WithName("Belgian InternalOrganisation")
                .Build();

            var takenByAddress = new PostalAddressBuilder(this.Transaction)
                .WithAddress1("address")
                .WithLocality("city")
                .WithCountry(new Countries(this.Transaction).CountryByIsoCode["BE"])
                .Build();

            var takenByContactMechanism = new PartyContactMechanismBuilder(this.Transaction)
                .WithParty(belgianInternalOrganisation)
                .WithContactMechanism(takenByAddress)
                .WithContactPurpose(new ContactMechanismPurposes(this.Transaction).RegisteredOffice)
                .WithContactPurpose(new ContactMechanismPurposes(this.Transaction).BillingAddress)
                .WithUseAsDefault(true)
                .Build();

            var shipFromAddress = new PostalAddressBuilder(this.Transaction)
                .WithAddress1("address")
                .WithLocality("city")
                .WithCountry(new Countries(this.Transaction).CountryByIsoCode["BE"])
                .Build();

            var shipFromContactMechanism = new PartyContactMechanismBuilder(this.Transaction)
                .WithParty(belgianInternalOrganisation)
                .WithContactMechanism(shipFromAddress)
                .WithContactPurpose(new ContactMechanismPurposes(this.Transaction).ShippingAddress)
                .WithUseAsDefault(true)
                .Build();

            new StoreBuilder(this.Transaction)
                .WithName("store")
                .WithBillingProcess(new BillingProcesses(this.Transaction).BillingForShipmentItems)
                .WithInternalOrganisation(belgianInternalOrganisation)
                .WithDefaultShipmentMethod(new ShipmentMethods(this.Transaction).Ground)
                .WithDefaultCarrier(new Carriers(this.Transaction).Fedex)
                .WithDefaultCollectionMethod(new PaymentMethods(this.Transaction).Extent().First())
                .WithIsImmediatelyPacked(true)
                .Build();

            var shipToAddress = new PostalAddressBuilder(this.Transaction)
                .WithAddress1("address")
                .WithLocality("city")
                .WithCountry(new Countries(this.Transaction).CountryByIsoCode["US"])
                .Build();

            var customer = new OrganisationBuilder(this.Transaction).WithName("customer").Build();
            new PartyContactMechanismBuilder(this.Transaction)
                .WithParty(customer)
                .WithContactMechanism(shipToAddress)
                .WithContactPurpose(new ContactMechanismPurposes(this.Transaction).ShippingAddress)
                .WithUseAsDefault(true)
                .Build();
            new CustomerRelationshipBuilder(this.Transaction).WithFromDate(this.Transaction.Now()).WithCustomer(customer).WithInternalOrganisation(belgianInternalOrganisation).Build();

            new UnifiedGoodBuilder(this.Transaction).WithSerialisedDefaults(belgianInternalOrganisation).Build();

            this.Transaction.Derive();

            // seller is belgian company, selling to outside EU customer, shipping From Belgium outside EU, seller responsible for transport
            var order = new SalesOrderBuilder(this.Transaction)
                .WithTakenBy(belgianInternalOrganisation)
                .WithBillToCustomer(customer)
                .WithShipToCustomer(customer)
                .WithAssignedShipToAddress(shipToAddress)
                .WithAssignedVatRegime(new VatRegimes(this.Transaction).ZeroRated)
                .WithSalesTerm(new IncoTermBuilder(this.Transaction).WithTermType(new IncoTermTypes(this.Transaction).Cif).Build())
                .Build();

            this.Transaction.Derive();

            var orderItem = new SalesOrderItemBuilder(this.Transaction).WithDefaults().Build();
            order.AddSalesOrderItem(orderItem);
            this.Transaction.Derive(false);

            Assert.Equal(new VatClauses(this.Transaction).BeArt39Par1Item1, order.DerivedVatClause);
        }

        [Fact]
        public void GivenSalesOrderTakenByBelgianInternalOrganisationForSellingToOutsideEUBusinessCustomerExw_WhenDerived_ThenVatClauseIsSet()
        {
            var belgianInternalOrganisation = new OrganisationBuilder(this.Transaction)
                .WithIsInternalOrganisation(true)
                .WithName("Belgian InternalOrganisation")
                .Build();

            var takenByAddress = new PostalAddressBuilder(this.Transaction)
                .WithAddress1("address")
                .WithLocality("city")
                .WithCountry(new Countries(this.Transaction).CountryByIsoCode["BE"])
                .Build();

            var takenByContactMechanism = new PartyContactMechanismBuilder(this.Transaction)
                .WithParty(belgianInternalOrganisation)
                .WithContactMechanism(takenByAddress)
                .WithContactPurpose(new ContactMechanismPurposes(this.Transaction).RegisteredOffice)
                .WithContactPurpose(new ContactMechanismPurposes(this.Transaction).BillingAddress)
                .WithUseAsDefault(true)
                .Build();

            var shipFromAddress = new PostalAddressBuilder(this.Transaction)
                .WithAddress1("address")
                .WithLocality("city")
                .WithCountry(new Countries(this.Transaction).CountryByIsoCode["BE"])
                .Build();

            var shipFromContactMechanism = new PartyContactMechanismBuilder(this.Transaction)
                .WithParty(belgianInternalOrganisation)
                .WithContactMechanism(shipFromAddress)
                .WithContactPurpose(new ContactMechanismPurposes(this.Transaction).ShippingAddress)
                .WithUseAsDefault(true)
                .Build();

            new StoreBuilder(this.Transaction)
                .WithName("store")
                .WithBillingProcess(new BillingProcesses(this.Transaction).BillingForShipmentItems)
                .WithInternalOrganisation(belgianInternalOrganisation)
                .WithDefaultShipmentMethod(new ShipmentMethods(this.Transaction).Ground)
                .WithDefaultCarrier(new Carriers(this.Transaction).Fedex)
                .WithDefaultCollectionMethod(new PaymentMethods(this.Transaction).Extent().First())
                .WithIsImmediatelyPacked(true)
                .Build();

            var shipToAddress = new PostalAddressBuilder(this.Transaction)
                .WithAddress1("address")
                .WithLocality("city")
                .WithCountry(new Countries(this.Transaction).CountryByIsoCode["US"])
                .Build();

            var customer = new OrganisationBuilder(this.Transaction).WithName("customer").Build();
            new PartyContactMechanismBuilder(this.Transaction)
                .WithParty(customer)
                .WithContactMechanism(shipToAddress)
                .WithContactPurpose(new ContactMechanismPurposes(this.Transaction).ShippingAddress)
                .WithUseAsDefault(true)
                .Build();
            new CustomerRelationshipBuilder(this.Transaction).WithFromDate(this.Transaction.Now()).WithCustomer(customer).WithInternalOrganisation(belgianInternalOrganisation).Build();

            new UnifiedGoodBuilder(this.Transaction).WithSerialisedDefaults(belgianInternalOrganisation).Build();

            this.Transaction.Derive();

            // seller is belgian company, selling to outside EU customer, shipping From Belgium outside EU, customer responsible for transport
            var order = new SalesOrderBuilder(this.Transaction)
                .WithTakenBy(belgianInternalOrganisation)
                .WithBillToCustomer(customer)
                .WithShipToCustomer(customer)
                .WithAssignedShipToAddress(shipToAddress)
                .WithAssignedVatRegime(new VatRegimes(this.Transaction).ZeroRated)
                .WithSalesTerm(new IncoTermBuilder(this.Transaction).WithTermType(new IncoTermTypes(this.Transaction).Exw).Build())
                .Build();

            this.Transaction.Derive();

            var orderItem = new SalesOrderItemBuilder(this.Transaction).WithDefaults().Build();
            order.AddSalesOrderItem(orderItem);
            this.Transaction.Derive(false);

            Assert.Equal(new VatClauses(this.Transaction).BeArt39Par1Item2, order.DerivedVatClause);
        }
    }

    public class SalesOrderDerivedVatClauseDescriptionRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public SalesOrderDerivedVatClauseDescriptionRuleTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedAssignedVatClauseDeriveDerivedVatClause()
        {
            var vatClause = new VatClauseBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var order = new SalesOrderBuilder(this.Transaction)
                .WithAssignedVatClause(vatClause)
                .Build();
            this.Transaction.Derive(false);

            Assert.Equal(vatClause, order.DerivedVatClause);
        }

        [Fact]
        public void ChangedDerivedVatRegimeDeriveDerivedVatClauseIsServiceB2B()
        {
            var order = new SalesOrderBuilder(this.Transaction)
                .WithAssignedVatRegime(new VatRegimes(this.Transaction).ServiceB2B)
                .Build();
            this.Transaction.Derive(false);

            Assert.Equal(new VatClauses(this.Transaction).ServiceB2B, order.DerivedVatClause);
        }

        [Fact]
        public void ChangedDerivedVatRegimeDeriveDerivedVatClauseIsIntracommunautair()
        {
            var order = new SalesOrderBuilder(this.Transaction)
                .WithAssignedVatRegime(new VatRegimes(this.Transaction).IntraCommunautair)
                .Build();
            this.Transaction.Derive(false);

            Assert.Equal(new VatClauses(this.Transaction).IntraCommunautair, order.DerivedVatClause);
        }

        [Fact]
        public void ChangedRegisteredOfficePostalAddressCountryDeriveDerivedVatClauseIsBeArt39Par1Item1()
        {
            var belgium = new Countries(this.Transaction).CountryByIsoCode["BE"];
            var shipToCountry = new Countries(this.Transaction).CountryByIsoCode["RU"];
            var registeredOfficeAddress = new PostalAddressBuilder(this.Transaction).Build();
            var shipFromAddress = new PostalAddressBuilder(this.Transaction).WithCountry(belgium).Build();
            var shipToAddress = new PostalAddressBuilder(this.Transaction).WithCountry(shipToCountry).Build();
            new PartyContactMechanismBuilder(this.Transaction).WithParty(this.InternalOrganisation).WithContactPurpose(new ContactMechanismPurposes(this.Transaction).RegisteredOffice).WithContactMechanism(registeredOfficeAddress).Build();
            new PartyContactMechanismBuilder(this.Transaction).WithParty(this.InternalOrganisation).WithContactPurpose(new ContactMechanismPurposes(this.Transaction).ShippingAddress).WithContactMechanism(shipFromAddress).WithUseAsDefault(true).Build();

            var billTo = new OrganisationBuilder(this.Transaction).Build();
            new PartyContactMechanismBuilder(this.Transaction).WithParty(billTo).WithContactPurpose(new ContactMechanismPurposes(this.Transaction).ShippingAddress).WithContactMechanism(shipToAddress).WithUseAsDefault(true).Build();
            this.Transaction.Derive(false);

            var order = new SalesOrderBuilder(this.Transaction)
                .WithTakenBy(this.InternalOrganisation)
                .WithBillToCustomer(billTo)
                .WithAssignedVatRegime(new VatRegimes(this.Transaction).ZeroRated)
                .WithSalesTerm(new IncoTermBuilder(this.Transaction).WithTermType(new IncoTermTypes(this.Transaction).Cif).Build())
                .Build();
            this.Transaction.Derive(false);

            var orderItem = new SalesOrderItemBuilder(this.Transaction).Build();
            order.AddSalesOrderItem(orderItem);
            this.Transaction.Derive(false);

            registeredOfficeAddress.Country = belgium;
            this.Transaction.Derive(false);

            Assert.Equal(new VatClauses(this.Transaction).BeArt39Par1Item1, order.DerivedVatClause);
        }

        [Fact]
        public void ChangedPartyContactMechanismContactPurposeDeriveDerivedVatClauseIsBeArt39Par1Item1()
        {
            var belgium = new Countries(this.Transaction).CountryByIsoCode["BE"];
            var shipToCountry = new Countries(this.Transaction).CountryByIsoCode["RU"];
            var registeredOfficeAddress = new PostalAddressBuilder(this.Transaction).WithCountry(belgium).Build();
            var shipFromAddress = new PostalAddressBuilder(this.Transaction).WithCountry(belgium).Build();
            var shipToAddress = new PostalAddressBuilder(this.Transaction).WithCountry(shipToCountry).Build();
            var registeredOffice = new PartyContactMechanismBuilder(this.Transaction).WithParty(this.InternalOrganisation).WithContactMechanism(registeredOfficeAddress).Build();
            var shipFrom = new PartyContactMechanismBuilder(this.Transaction).WithParty(this.InternalOrganisation).WithContactPurpose(new ContactMechanismPurposes(this.Transaction).ShippingAddress).WithContactMechanism(shipFromAddress).WithUseAsDefault(true).Build();
            this.Transaction.Derive(false);

            var billTo = new OrganisationBuilder(this.Transaction).Build();
            new PartyContactMechanismBuilder(this.Transaction).WithParty(billTo).WithContactPurpose(new ContactMechanismPurposes(this.Transaction).ShippingAddress).WithContactMechanism(shipToAddress).WithUseAsDefault(true).Build();

            var order = new SalesOrderBuilder(this.Transaction)
                .WithTakenBy(this.InternalOrganisation)
                .WithBillToCustomer(billTo)
                .WithAssignedVatRegime(new VatRegimes(this.Transaction).ZeroRated)
                .WithSalesTerm(new IncoTermBuilder(this.Transaction).WithTermType(new IncoTermTypes(this.Transaction).Cif).Build())
                .Build();
            this.Transaction.Derive(false);

            var orderItem = new SalesOrderItemBuilder(this.Transaction).Build();
            order.AddSalesOrderItem(orderItem);
            this.Transaction.Derive(false);

            registeredOffice.AddContactPurpose(new ContactMechanismPurposes(this.Transaction).RegisteredOffice);
            this.Transaction.Derive(false);

            Assert.Equal(new VatClauses(this.Transaction).BeArt39Par1Item1, order.DerivedVatClause);
        }

        [Fact]
        public void ChangedPartyPartyContactMechanismDeriveDerivedVatClauseIsBeArt39Par1Item1()
        {
            var belgium = new Countries(this.Transaction).CountryByIsoCode["BE"];
            var shipToCountry = new Countries(this.Transaction).CountryByIsoCode["RU"];
            var registeredOfficeAddress = new PostalAddressBuilder(this.Transaction).WithCountry(belgium).Build();
            var shipFromAddress = new PostalAddressBuilder(this.Transaction).WithCountry(belgium).Build();
            var shipToAddress = new PostalAddressBuilder(this.Transaction).WithCountry(shipToCountry).Build();
            var registeredOffice = new PartyContactMechanismBuilder(this.Transaction).WithContactPurpose(new ContactMechanismPurposes(this.Transaction).RegisteredOffice).WithContactMechanism(registeredOfficeAddress).Build();
            var shipFrom = new PartyContactMechanismBuilder(this.Transaction).WithParty(this.InternalOrganisation).WithContactPurpose(new ContactMechanismPurposes(this.Transaction).ShippingAddress).WithContactMechanism(shipFromAddress).WithUseAsDefault(true).Build();

            var billTo = new OrganisationBuilder(this.Transaction).Build();
            new PartyContactMechanismBuilder(this.Transaction).WithParty(billTo).WithContactPurpose(new ContactMechanismPurposes(this.Transaction).ShippingAddress).WithContactMechanism(shipToAddress).WithUseAsDefault(true).Build();
            this.Transaction.Derive(false);

            var order = new SalesOrderBuilder(this.Transaction)
                .WithTakenBy(this.InternalOrganisation)
                .WithBillToCustomer(billTo)
                .WithAssignedVatRegime(new VatRegimes(this.Transaction).ZeroRated)
                .WithSalesTerm(new IncoTermBuilder(this.Transaction).WithTermType(new IncoTermTypes(this.Transaction).Cif).Build())
                .Build();
            this.Transaction.Derive(false);

            var orderItem = new SalesOrderItemBuilder(this.Transaction).Build();
            order.AddSalesOrderItem(orderItem);
            this.Transaction.Derive(false);

            registeredOffice.Party = this.InternalOrganisation;
            this.Transaction.Derive(false);

            Assert.Equal(new VatClauses(this.Transaction).BeArt39Par1Item1, order.DerivedVatClause);
        }

        [Fact]
        public void ChangedBillToCustomerDeriveDerivedVatClauseIsBeArt39Par1Item1()
        {
            var belgium = new Countries(this.Transaction).CountryByIsoCode["BE"];
            var shipToCountry = new Countries(this.Transaction).CountryByIsoCode["RU"];
            var registeredOfficeAddress = new PostalAddressBuilder(this.Transaction).WithCountry(belgium).Build();
            var shipFromAddress = new PostalAddressBuilder(this.Transaction).WithCountry(belgium).Build();
            var shipToAddress = new PostalAddressBuilder(this.Transaction).WithCountry(shipToCountry).Build();
            var registeredOffice = new PartyContactMechanismBuilder(this.Transaction).WithParty(this.InternalOrganisation).WithContactPurpose(new ContactMechanismPurposes(this.Transaction).RegisteredOffice).WithContactMechanism(registeredOfficeAddress).Build();
            var shipFrom = new PartyContactMechanismBuilder(this.Transaction).WithParty(this.InternalOrganisation).WithContactPurpose(new ContactMechanismPurposes(this.Transaction).ShippingAddress).WithContactMechanism(shipFromAddress).WithUseAsDefault(true).Build();

            var billTo = new OrganisationBuilder(this.Transaction).Build();
            new PartyContactMechanismBuilder(this.Transaction).WithParty(billTo).WithContactPurpose(new ContactMechanismPurposes(this.Transaction).ShippingAddress).WithContactMechanism(shipToAddress).WithUseAsDefault(true).Build();
            this.Transaction.Derive(false);

            var order = new SalesOrderBuilder(this.Transaction)
                .WithTakenBy(this.InternalOrganisation)
                .WithAssignedVatRegime(new VatRegimes(this.Transaction).ZeroRated)
                .WithSalesTerm(new IncoTermBuilder(this.Transaction).WithTermType(new IncoTermTypes(this.Transaction).Cif).Build())
                .Build();
            this.Transaction.Derive(false);

            var orderItem = new SalesOrderItemBuilder(this.Transaction).Build();
            order.AddSalesOrderItem(orderItem);
            this.Transaction.Derive(false);

            order.BillToCustomer = billTo;
            this.Transaction.Derive(false);

            Assert.Equal(new VatClauses(this.Transaction).BeArt39Par1Item1, order.DerivedVatClause);
        }

        [Fact]
        public void ChangedValidOrderItemsDeriveDerivedVatClauseIsBeArt39Par1Item1()
        {
            var belgium = new Countries(this.Transaction).CountryByIsoCode["BE"];
            var shipToCountry = new Countries(this.Transaction).CountryByIsoCode["RU"];
            var registeredOfficeAddress = new PostalAddressBuilder(this.Transaction).WithCountry(belgium).Build();
            var shipFromAddress = new PostalAddressBuilder(this.Transaction).WithCountry(belgium).Build();
            var shipToAddress = new PostalAddressBuilder(this.Transaction).WithCountry(shipToCountry).Build();
            var registeredOffice = new PartyContactMechanismBuilder(this.Transaction).WithParty(this.InternalOrganisation).WithContactPurpose(new ContactMechanismPurposes(this.Transaction).RegisteredOffice).WithContactMechanism(registeredOfficeAddress).Build();
            var shipFrom = new PartyContactMechanismBuilder(this.Transaction).WithParty(this.InternalOrganisation).WithContactPurpose(new ContactMechanismPurposes(this.Transaction).ShippingAddress).WithContactMechanism(shipFromAddress).WithUseAsDefault(true).Build();

            var billTo = new OrganisationBuilder(this.Transaction).Build();
            new PartyContactMechanismBuilder(this.Transaction).WithParty(billTo).WithContactPurpose(new ContactMechanismPurposes(this.Transaction).ShippingAddress).WithContactMechanism(shipToAddress).WithUseAsDefault(true).Build();
            this.Transaction.Derive(false);

            var order = new SalesOrderBuilder(this.Transaction)
                .WithTakenBy(this.InternalOrganisation)
                .WithBillToCustomer(billTo)
                .WithAssignedVatRegime(new VatRegimes(this.Transaction).ZeroRated)
                .WithSalesTerm(new IncoTermBuilder(this.Transaction).WithTermType(new IncoTermTypes(this.Transaction).Cif).Build())
                .Build();
            this.Transaction.Derive(false);

            var orderItem = new SalesOrderItemBuilder(this.Transaction).WithSalesOrderItemState(new SalesOrderItemStates(this.Transaction).Cancelled).Build();
            order.AddSalesOrderItem(orderItem);
            this.Transaction.Derive(false);

            orderItem.SalesOrderItemState = new SalesOrderItemStates(this.Transaction).Provisional;
            this.Transaction.Derive(false);

            Assert.Equal(new VatClauses(this.Transaction).BeArt39Par1Item1, order.DerivedVatClause);
        }

        [Fact]
        public void ChangedOrderItemDerivedShipFromAddressDeriveDerivedVatClauseIsBeArt39Par1Item1()
        {
            var belgium = new Countries(this.Transaction).CountryByIsoCode["BE"];
            var shipToCountry = new Countries(this.Transaction).CountryByIsoCode["RU"];
            var registeredOfficeAddress = new PostalAddressBuilder(this.Transaction).WithCountry(belgium).Build();
            var shipFromAddress = new PostalAddressBuilder(this.Transaction).WithCountry(belgium).Build();
            var shipToAddress = new PostalAddressBuilder(this.Transaction).WithCountry(shipToCountry).Build();
            var registeredOffice = new PartyContactMechanismBuilder(this.Transaction).WithParty(this.InternalOrganisation).WithContactPurpose(new ContactMechanismPurposes(this.Transaction).RegisteredOffice).WithContactMechanism(registeredOfficeAddress).Build();
            var shipFrom = new PartyContactMechanismBuilder(this.Transaction).WithContactPurpose(new ContactMechanismPurposes(this.Transaction).ShippingAddress).WithContactMechanism(shipFromAddress).WithUseAsDefault(true).Build();

            var billTo = new OrganisationBuilder(this.Transaction).Build();
            new PartyContactMechanismBuilder(this.Transaction).WithParty(billTo).WithContactPurpose(new ContactMechanismPurposes(this.Transaction).ShippingAddress).WithContactMechanism(shipToAddress).WithUseAsDefault(true).Build();
            this.Transaction.Derive(false);

            var order = new SalesOrderBuilder(this.Transaction)
                .WithTakenBy(this.InternalOrganisation)
                .WithBillToCustomer(billTo)
                .WithAssignedVatRegime(new VatRegimes(this.Transaction).ZeroRated)
                .WithSalesTerm(new IncoTermBuilder(this.Transaction).WithTermType(new IncoTermTypes(this.Transaction).Cif).Build())
                .Build();
            this.Transaction.Derive(false);

            var orderItem = new SalesOrderItemBuilder(this.Transaction).Build();
            order.AddSalesOrderItem(orderItem);
            this.Transaction.Derive(false);

            orderItem.AssignedShipFromAddress = shipFromAddress;
            this.Transaction.Derive(false);

            Assert.Equal(new VatClauses(this.Transaction).BeArt39Par1Item1, order.DerivedVatClause);
        }

        [Fact]
        public void ChangedShipFromPostalAddressCountryDeriveDerivedVatClauseIsBeArt39Par1Item1()
        {
            var belgium = new Countries(this.Transaction).CountryByIsoCode["BE"];
            var shipToCountry = new Countries(this.Transaction).CountryByIsoCode["RU"];
            var registeredOfficeAddress = new PostalAddressBuilder(this.Transaction).WithCountry(belgium).Build();
            var shipFromAddress = new PostalAddressBuilder(this.Transaction).Build();
            var shipToAddress = new PostalAddressBuilder(this.Transaction).WithCountry(shipToCountry).Build();
            var registeredOffice = new PartyContactMechanismBuilder(this.Transaction).WithParty(this.InternalOrganisation).WithContactPurpose(new ContactMechanismPurposes(this.Transaction).RegisteredOffice).WithContactMechanism(registeredOfficeAddress).Build();
            var shipFrom = new PartyContactMechanismBuilder(this.Transaction).WithParty(this.InternalOrganisation).WithContactPurpose(new ContactMechanismPurposes(this.Transaction).ShippingAddress).WithContactMechanism(shipFromAddress).WithUseAsDefault(true).Build();

            var billTo = new OrganisationBuilder(this.Transaction).Build();
            new PartyContactMechanismBuilder(this.Transaction).WithParty(billTo).WithContactPurpose(new ContactMechanismPurposes(this.Transaction).ShippingAddress).WithContactMechanism(shipToAddress).WithUseAsDefault(true).Build();
            this.Transaction.Derive(false);

            var order = new SalesOrderBuilder(this.Transaction)
                .WithTakenBy(this.InternalOrganisation)
                .WithBillToCustomer(billTo)
                .WithAssignedVatRegime(new VatRegimes(this.Transaction).ZeroRated)
                .WithSalesTerm(new IncoTermBuilder(this.Transaction).WithTermType(new IncoTermTypes(this.Transaction).Cif).Build())
                .Build();
            this.Transaction.Derive(false);

            var orderItem = new SalesOrderItemBuilder(this.Transaction).Build();
            order.AddSalesOrderItem(orderItem);
            this.Transaction.Derive(false);

            shipFromAddress.Country = belgium;
            this.Transaction.Derive(false);

            Assert.Equal(new VatClauses(this.Transaction).BeArt39Par1Item1, order.DerivedVatClause);
        }

        [Fact]
        public void ChangedShipToPostalAddressCountryDeriveDerivedVatClauseIsBeArt39Par1Item1()
        {
            var belgium = new Countries(this.Transaction).CountryByIsoCode["BE"];
            var shipToCountry = new Countries(this.Transaction).CountryByIsoCode["NL"];
            var registeredOfficeAddress = new PostalAddressBuilder(this.Transaction).WithCountry(belgium).Build();
            var shipFromAddress = new PostalAddressBuilder(this.Transaction).WithCountry(belgium).Build();
            var shipToAddress = new PostalAddressBuilder(this.Transaction).WithCountry(shipToCountry).Build();
            var registeredOffice = new PartyContactMechanismBuilder(this.Transaction).WithParty(this.InternalOrganisation).WithContactPurpose(new ContactMechanismPurposes(this.Transaction).RegisteredOffice).WithContactMechanism(registeredOfficeAddress).Build();
            var shipFrom = new PartyContactMechanismBuilder(this.Transaction).WithParty(this.InternalOrganisation).WithContactPurpose(new ContactMechanismPurposes(this.Transaction).ShippingAddress).WithContactMechanism(shipFromAddress).WithUseAsDefault(true).Build();

            var billTo = new OrganisationBuilder(this.Transaction).Build();
            new PartyContactMechanismBuilder(this.Transaction).WithParty(billTo).WithContactPurpose(new ContactMechanismPurposes(this.Transaction).ShippingAddress).WithContactMechanism(shipToAddress).WithUseAsDefault(true).Build();
            this.Transaction.Derive(false);

            var order = new SalesOrderBuilder(this.Transaction)
                .WithTakenBy(this.InternalOrganisation)
                .WithBillToCustomer(billTo)
                .WithAssignedVatRegime(new VatRegimes(this.Transaction).ZeroRated)
                .WithSalesTerm(new IncoTermBuilder(this.Transaction).WithTermType(new IncoTermTypes(this.Transaction).Cif).Build())
                .Build();
            this.Transaction.Derive(false);

            var orderItem = new SalesOrderItemBuilder(this.Transaction).Build();
            order.AddSalesOrderItem(orderItem);
            this.Transaction.Derive(false);

            shipToAddress.Country = new Countries(this.Transaction).CountryByIsoCode["RU"];
            this.Transaction.Derive(false);

            Assert.Equal(new VatClauses(this.Transaction).BeArt39Par1Item1, order.DerivedVatClause);
        }

        [Fact]
        public void ChangedSalesTermDeriveDerivedVatClauseIsBeArt39Par1Item1()
        {
            var belgium = new Countries(this.Transaction).CountryByIsoCode["BE"];
            var shipToCountry = new Countries(this.Transaction).CountryByIsoCode["RU"];
            var registeredOfficeAddress = new PostalAddressBuilder(this.Transaction).WithCountry(belgium).Build();
            var shipFromAddress = new PostalAddressBuilder(this.Transaction).WithCountry(belgium).Build();
            var shipToAddress = new PostalAddressBuilder(this.Transaction).WithCountry(shipToCountry).Build();
            var registeredOffice = new PartyContactMechanismBuilder(this.Transaction).WithParty(this.InternalOrganisation).WithContactPurpose(new ContactMechanismPurposes(this.Transaction).RegisteredOffice).WithContactMechanism(registeredOfficeAddress).Build();
            var shipFrom = new PartyContactMechanismBuilder(this.Transaction).WithParty(this.InternalOrganisation).WithContactPurpose(new ContactMechanismPurposes(this.Transaction).ShippingAddress).WithContactMechanism(shipFromAddress).WithUseAsDefault(true).Build();

            var billTo = new OrganisationBuilder(this.Transaction).Build();
            new PartyContactMechanismBuilder(this.Transaction).WithParty(billTo).WithContactPurpose(new ContactMechanismPurposes(this.Transaction).ShippingAddress).WithContactMechanism(shipToAddress).WithUseAsDefault(true).Build();
            this.Transaction.Derive(false);

            var order = new SalesOrderBuilder(this.Transaction)
                .WithTakenBy(this.InternalOrganisation)
                .WithBillToCustomer(billTo)
                .WithAssignedVatRegime(new VatRegimes(this.Transaction).ZeroRated)
                .Build();
            this.Transaction.Derive(false);

            var orderItem = new SalesOrderItemBuilder(this.Transaction).Build();
            order.AddSalesOrderItem(orderItem);
            this.Transaction.Derive(false);

            order.AddSalesTerm(new IncoTermBuilder(this.Transaction).WithTermType(new IncoTermTypes(this.Transaction).Cif).Build());
            this.Transaction.Derive(false);

            Assert.Equal(new VatClauses(this.Transaction).BeArt39Par1Item1, order.DerivedVatClause);
        }

        [Fact]
        public void ChangedSalesTermTermTypeDeriveDerivedVatClauseIsBeArt39Par1Item1()
        {
            var belgium = new Countries(this.Transaction).CountryByIsoCode["BE"];
            var shipToCountry = new Countries(this.Transaction).CountryByIsoCode["RU"];
            var registeredOfficeAddress = new PostalAddressBuilder(this.Transaction).WithCountry(belgium).Build();
            var shipFromAddress = new PostalAddressBuilder(this.Transaction).WithCountry(belgium).Build();
            var shipToAddress = new PostalAddressBuilder(this.Transaction).WithCountry(shipToCountry).Build();
            var registeredOffice = new PartyContactMechanismBuilder(this.Transaction).WithParty(this.InternalOrganisation).WithContactPurpose(new ContactMechanismPurposes(this.Transaction).RegisteredOffice).WithContactMechanism(registeredOfficeAddress).Build();
            var shipFrom = new PartyContactMechanismBuilder(this.Transaction).WithParty(this.InternalOrganisation).WithContactPurpose(new ContactMechanismPurposes(this.Transaction).ShippingAddress).WithContactMechanism(shipFromAddress).WithUseAsDefault(true).Build();

            var billTo = new OrganisationBuilder(this.Transaction).Build();
            new PartyContactMechanismBuilder(this.Transaction).WithParty(billTo).WithContactPurpose(new ContactMechanismPurposes(this.Transaction).ShippingAddress).WithContactMechanism(shipToAddress).WithUseAsDefault(true).Build();
            this.Transaction.Derive(false);

            var order = new SalesOrderBuilder(this.Transaction)
                .WithTakenBy(this.InternalOrganisation)
                .WithBillToCustomer(billTo)
                .WithAssignedVatRegime(new VatRegimes(this.Transaction).ZeroRated)
                .WithSalesTerm(new IncoTermBuilder(this.Transaction).WithTermType(new IncoTermTypes(this.Transaction).Dap).Build())
                .Build();
            this.Transaction.Derive(false);

            var orderItem = new SalesOrderItemBuilder(this.Transaction).Build();
            order.AddSalesOrderItem(orderItem);
            this.Transaction.Derive(false);

            order.SalesTerms.First().TermType = new IncoTermTypes(this.Transaction).Cif;
            this.Transaction.Derive(false);

            Assert.Equal(new VatClauses(this.Transaction).BeArt39Par1Item1, order.DerivedVatClause);
        }
    }
}
