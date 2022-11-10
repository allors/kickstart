namespace Allors.Database.Domain.Tests
{
    using Xunit;

    public class SalesInvoiceTest : DomainTest, IClassFixture<Fixture>
    {
        public SalesInvoiceTest(Fixture fixture) : base(fixture) { }

        [Fact]
        public void PrintContent()
        {
            var customer = new OrganisationBuilder(this.Transaction).WithName("customer").Build();

            new CustomerRelationshipBuilder(this.Transaction).WithCustomer(customer).WithInternalOrganisation(this.InternalOrganisation).Build();

            var contactMechanism = new PostalAddressBuilder(this.Transaction)
                .WithAddress1("Haverwerf 15")
                .WithLocality("Mechelen")
                .WithCountry(new Countries(this.Transaction).FindBy(M.Country.IsoCode, "BE"))
                .Build();

            var invoice = new SalesInvoiceBuilder(this.Transaction)
                .WithInvoiceNumber("1")
                .WithBillToCustomer(customer)
                .WithAssignedBillToEndCustomerContactMechanism(contactMechanism)
                .WithSalesInvoiceType(new SalesInvoiceTypes(this.Transaction).SalesInvoice)
                .Build();

            // TODO: ??
            //var content = invoice.HtmlContent;
        }

        [Fact]
        public void DeriveSecurityTokens()
        {
            var invoice = new SalesInvoiceBuilder(this.Transaction).WithBilledFrom(this.InternalOrganisation).Build();
            this.Transaction.Derive(false);

            Assert.Contains(new SecurityTokens(this.Transaction).DefaultSecurityToken, invoice.SecurityTokens);
            Assert.Contains(this.InternalOrganisation.LocalAdministratorSecurityToken, invoice.SecurityTokens);
            Assert.Contains(this.InternalOrganisation.LocalSalesAccountManagerSecurityToken, invoice.SecurityTokens);
        }
    }
}
