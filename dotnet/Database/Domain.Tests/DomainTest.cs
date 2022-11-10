// <copyright file="DomainTest.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the DomainTest type.</summary>

using Allors.Database.Configuration.Derivations.Default;
using Allors.Database.Meta;

namespace Allors.Database.Domain.Tests
{
    using System;
    using System.Globalization;
    using System.Linq;
    using Adapters.Memory;
    using Domain;
    using Configuration;
    using Database;
    using TestPopulation;
    using Allors.Database.Derivations;

    public class DomainTest : IDisposable
    {
        public DomainTest(Fixture fixture, bool populate = true)
        {
            var database = new Database(
                new TestDatabaseServices(fixture.Engine, null),
                new Configuration
                {
                    ObjectFactory = new ObjectFactory(fixture.M, typeof(User)),
                });

            this.M = database.Services.Get<MetaPopulation>();

            this.Setup(database, populate);
        }

        public MetaPopulation M { get; }

        public virtual Config Config { get; } = new Config { SetupSecurity = false };

        public ITransaction Transaction { get; private set; }

        public ITime Time => this.Transaction.Database.Services.Get<ITime>();

        public ISecurity Security => this.Transaction.Database.Services.Get<ISecurity>();

        public TimeSpan? TimeShift
        {
            get => this.Time.Shift;

            set => this.Time.Shift = value;
        }

        protected Organisation InternalOrganisation => this.Transaction.Extent<Organisation>().First(v => v.IsInternalOrganisation && v.Name == "internalOrganisation");

        protected Store Store => this.InternalOrganisation.StoresWhereInternalOrganisation.First();

        protected Facility DefaultFacility => this.InternalOrganisation.FacilitiesWhereOwner.First();

        protected Person Administrator => (Person)new UserGroups(this.Transaction).Administrators.Members.First();

        protected Person OrderProcessor => this.GetPersonByUserName("orderProcessor");

        protected Person Purchaser => this.GetPersonByUserName("purchaser");

        protected Person Employee => this.InternalOrganisation.ActiveEmployees.First();

        protected Organisation Customer => (Organisation)this.InternalOrganisation.ActiveCustomers.First(v => v.GetType().Name.Equals(typeof(Organisation).Name));

        protected Organisation Supplier => this.InternalOrganisation.ActiveSuppliers.First();

        protected Organisation SubContractor => this.InternalOrganisation.ActiveSubContractors.First();

        public void Dispose()
        {
            this.Transaction.Rollback();
            this.Transaction = null;
        }

        protected IValidation Derive() => this.Transaction.Derive(false, true);

        protected void Setup(IDatabase database, bool populate)
        {
            database.Init();

            this.Transaction = database.CreateTransaction();

            if (populate)
            {
                this.Populate(database);
            }
        }

        private void Populate(IDatabase database)
        {
            CultureInfo.CurrentCulture = new CultureInfo("en-GB");
            CultureInfo.CurrentUICulture = new CultureInfo("en-GB");

            new Setup(database, this.Config).Apply();

            this.Transaction.Rollback();

            var administrator = new People(this.Transaction).FindBy(M.Person.UserName, "JaneDoe");

            if (administrator == null)
            {
                administrator = new PersonBuilder(this.Transaction)
                    .WithFirstName("Jane")
                    .WithLastName("Doe")
                    .WithUserName("JaneDoe")
                    .Build();

                new UserGroups(this.Transaction).Administrators.AddMember(administrator);
            }

            this.Transaction.Services.Get<IUserService>().User = administrator;

            var singleton = this.Transaction.GetSingleton();

            this.Transaction.Derive();
            this.Transaction.Commit();

            if (new Organisations(this.Transaction).FindBy(M.Organisation.Name, "internalOrganisation") == null)
            {
                var belgium = new Countries(this.Transaction).CountryByIsoCode["BE"];
                var euro = belgium.Currency;

                var bank = new BankBuilder(this.Transaction).WithCountry(belgium).WithName("ING België").WithBic("BBRUBEBB").Build();

                var ownBankAccount = new OwnBankAccountBuilder(this.Transaction)
                    .WithBankAccount(new BankAccountBuilder(this.Transaction).WithBank(bank)
                                        .WithCurrency(euro)
                                        .WithIban("BE68539007547034")
                                        .WithNameOnAccount("Koen")
                                        .Build())
                    .WithDescription("Main bank account")
                    .Build();

                var postalAddress = new PostalAddressBuilder(this.Transaction).WithAddress1("Kleine Nieuwedijkstraat 2").WithLocality("Mechelen").WithCountry(belgium).Build();

                var internalOrganisation = new OrganisationBuilder(this.Transaction)
                    .WithIsInternalOrganisation(true)
                    .WithName("internalOrganisation")
                    .WithPreferredCurrency(new Currencies(this.Transaction).CurrencyByCode["EUR"])
                    .WithInvoiceSequence(new InvoiceSequences(this.Transaction).EnforcedSequence)
                    .WithRequestSequence(new RequestSequences(this.Transaction).EnforcedSequence)
                    .WithQuoteSequence(new QuoteSequences(this.Transaction).EnforcedSequence)
                    .WithCustomerShipmentSequence(new CustomerShipmentSequences(this.Transaction).EnforcedSequence)
                    .WithCustomerReturnSequence(new CustomerReturnSequences(this.Transaction).EnforcedSequence)
                    .WithPurchaseShipmentSequence(new PurchaseShipmentSequences(this.Transaction).EnforcedSequence)
                    .WithPurchaseReturnSequence(new PurchaseReturnSequences(this.Transaction).EnforcedSequence)
                    .WithDropShipmentSequence(new DropShipmentSequences(this.Transaction).EnforcedSequence)
                    .WithIncomingTransferSequence(new IncomingTransferSequences(this.Transaction).EnforcedSequence)
                    .WithOutgoingTransferSequence(new OutgoingTransferSequences(this.Transaction).EnforcedSequence)
                    .WithWorkEffortSequence(new WorkEffortSequences(this.Transaction).EnforcedSequence)
                    .WithPurchaseShipmentNumberPrefix("incoming shipmentno: ")
                    .WithPurchaseInvoiceNumberPrefix("incoming invoiceno: ")
                    .WithPurchaseOrderNumberPrefix("purchase orderno: ")
                    .WithDefaultCollectionMethod(ownBankAccount)
                    .Build();

                this.Transaction.Derive();
                this.Transaction.Commit();

                new PartyContactMechanismBuilder(this.Transaction).WithParty(this.InternalOrganisation).WithContactMechanism(postalAddress).WithContactPurpose(new ContactMechanismPurposes(this.Transaction).GeneralCorrespondence).WithUseAsDefault(true).Build();

                var facility = new FacilityBuilder(this.Transaction).WithFacilityType(new FacilityTypes(this.Transaction).Warehouse).WithName("facility").WithOwner(internalOrganisation).Build();

                var paymentMethod = new PaymentMethods(this.Transaction).Extent().First();

                new StoreBuilder(this.Transaction)
                    .WithName("store")
                    .WithInternalOrganisation(internalOrganisation)
                    .WithCustomerShipmentNumberPrefix("shipmentno: ")
                    .WithSalesInvoiceNumberPrefix("invoiceno: ")
                    .WithSalesOrderNumberPrefix("orderno: ")
                    .WithDefaultShipmentMethod(new ShipmentMethods(this.Transaction).Ground)
                    .WithDefaultCarrier(new Carriers(this.Transaction).Fedex)
                    .WithCreditLimit(500)
                    .WithPaymentGracePeriod(10)
                    .WithDefaultCollectionMethod(paymentMethod)
                    .WithDefaultFacility(facility)
                    .Build();

                internalOrganisation.CreateB2BCustomer(this.Transaction.Faker());
                internalOrganisation.CreateB2CCustomer(this.Transaction.Faker());
                internalOrganisation.CreateSupplier(this.Transaction.Faker());
                internalOrganisation.CreateSubContractor(this.Transaction.Faker());
                internalOrganisation.CreateEmployee("letmein", this.Transaction.Faker());
                internalOrganisation.CreateEmployee("letmein", this.Transaction.Faker());

                this.Transaction.Derive();
                this.Transaction.Commit();
            }
        }

        private Person GetPersonByUserName(string userName) => new People(this.Transaction).FindBy(this.M.User.UserName, userName);
    }
}
