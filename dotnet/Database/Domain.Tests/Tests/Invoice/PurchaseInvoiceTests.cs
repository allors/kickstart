namespace Allors.Database.Domain.Tests
{
    using Xunit;

    public class PurchaseInvoiceTest : DomainTest, IClassFixture<Fixture>
    {
        public PurchaseInvoiceTest(Fixture fixture) : base(fixture) { }

        [Fact]
        public void DeriveSecurityTokens()
        {
            var invoice = new PurchaseInvoiceBuilder(this.Transaction).WithBilledTo(this.InternalOrganisation).Build();
            this.Transaction.Derive(false);

            Assert.Contains(new SecurityTokens(this.Transaction).DefaultSecurityToken, invoice.SecurityTokens);
            Assert.Contains(this.InternalOrganisation.LocalSalesAccountManagerSecurityToken, invoice.SecurityTokens);
            Assert.Contains(this.InternalOrganisation.LocalAdministratorSecurityToken, invoice.SecurityTokens);
            Assert.Contains(this.InternalOrganisation.PurchaseInvoiceApproverSecurityToken, invoice.SecurityTokens);
        }
    }
}
