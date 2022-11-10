// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Fixture.cs" company="Allors bvba">
//   Copyright 2002-2009 Allors bvba.
// 
// Dual Licensed under
//   a) the General Public Licence v3 (GPL)
//   b) the Allors License
// 
// The GPL License is included in the file gpl.txt.
// The Allors License is an addendum to your contract.
// 
// Allors Platform is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// For more information visit http://www.allors.com/legal
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Allors
{
    using System;
    using System.Globalization;
    using Domain;

    public class Fixture
    {
        public static void Setup(IDatabase database)
        {
            CultureInfo.CurrentCulture = new CultureInfo("en-GB");
            CultureInfo.CurrentUICulture = new CultureInfo("en-GB");

            using (var session = database.CreateSession())
            {
                var config = new Config { SetupSecurity = true};
                new Setup(session, config).Apply();

                var administrator = new PersonBuilder(session)
                    .WithFirstName("Jane")
                    .WithLastName("Doe")
                    .WithUserName("administrator")
                    .Build();

                new UserGroups(session).Administrators.AddMember(administrator);

                session.SetUser(administrator);

                var singleton = session.GetSingleton();

                session.Derive();
                session.Commit();

                var belgium = new Countries(session).CountryByIsoCode["BE"];
                var euro = belgium.Currency;

                var bank = new BankBuilder(session).WithCountry(belgium).WithName("ING België").WithBic("BBRUBEBB").Build();

                var ownBankAccount = new OwnBankAccountBuilder(session)
                    .WithBankAccount(new BankAccountBuilder(session).WithBank(bank)
                                        .WithCurrency(euro)
                                        .WithIban("BE68539007547034")
                                        .WithNameOnAccount("Koen")
                                        .Build())
                    .WithDescription("Main bank account")
                    .Build();

                var postalAddress = new PostalAddressBuilder(session).WithAddress1("Kleine Nieuwedijkstraat 2").WithLocality("Mechelen").WithCountry(belgium).Build();

                var internalOrganisation = new OrganisationBuilder(session)
                    .WithIsInternalOrganisation(true)
                    .WithName("internalOrganisation")
                    .WithPreferredCurrency(new Currencies(session).CurrencyByCode["EUR"])
                    .WithIncomingShipmentNumberPrefix("incoming shipmentno: ")
                    .WithPurchaseInvoiceNumberPrefix("incoming invoiceno: ")
                    .WithPurchaseOrderNumberPrefix("purchase orderno: ")
                    .WithDefaultCollectionMethod(ownBankAccount)
                    .Build();

                var facility = new FacilityBuilder(session).WithFacilityType(new FacilityTypes(session).Warehouse).WithName("facility").WithOwner(internalOrganisation).Build();

                var paymentMethod = new PaymentMethods(session).Extent().First;

                new StoreBuilder(session)
                    .WithName("store")
                    .WithInternalOrganisation(internalOrganisation)
                    .WithOutgoingShipmentNumberPrefix("shipmentno: ")
                    .WithSalesInvoiceNumberPrefix("invoiceno: ")
                    .WithSalesOrderNumberPrefix("orderno: ")
                    .WithDefaultShipmentMethod(new ShipmentMethods(session).Ground)
                    .WithDefaultCarrier(new Carriers(session).Fedex)
                    .WithCreditLimit(500)
                    .WithPaymentGracePeriod(10)
                    .WithDefaultCollectionMethod(paymentMethod)
                    .WithDefaultFacility(facility)
                    .Build();

                var customer = new OrganisationBuilder(session).WithName("customer").WithLocale(singleton.DefaultLocale).Build();
                var supplier = new OrganisationBuilder(session).WithName("supplier").WithLocale(singleton.DefaultLocale).Build();
                var purchaser = new PersonBuilder(session).WithLastName("purchaser").WithUserName("purchaser").Build();
                var orderProcessor = new PersonBuilder(session).WithLastName("orderProcessor").WithUserName("orderProcessor").Build();

                new CustomerRelationshipBuilder(session).WithCustomer(customer).WithInternalOrganisation(internalOrganisation).WithFromDate(DateTime.UtcNow).Build();

                new SupplierRelationshipBuilder(session).WithSupplier(supplier).WithInternalOrganisation(internalOrganisation).WithFromDate(DateTime.UtcNow).Build();

                new EmploymentBuilder(session).WithFromDate(DateTime.UtcNow).WithEmployee(orderProcessor).WithEmployer(internalOrganisation).Build();

                session.Derive();
                session.Commit();
            }
        }
    }
}