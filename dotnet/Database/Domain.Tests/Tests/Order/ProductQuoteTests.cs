using System.Linq;

namespace Allors.Database.Domain.Tests
{
    using Allors.Database.Derivations;
    using System.Collections.Generic;
    using Xunit;

    public class ProductQuoteTests : DomainTest, IClassFixture<Fixture>
    {
        public ProductQuoteTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void GivenQuoteToRent_WhenOrdered_ThenOrderItemNextSerialisedItemAvailabilityIsInRent()
        {
            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();

            var quote = new ProductQuoteBuilder(this.Transaction).WithIssuer(this.InternalOrganisation).Build();
            this.Transaction.Derive(false);

            var quoteItem = new QuoteItemBuilder(this.Transaction)
                .WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductItem)
                .WithSerialisedItem(serialisedItem)
                .WithRentalType(new RentalTypes(this.Transaction).Extent().First())
                .Build();

            quote.AddQuoteItem(quoteItem);
            this.Transaction.Derive(false);

            quote.SetReadyForProcessing();
            this.Transaction.Derive(false);

            quote.Approve();
            this.Transaction.Derive(false);

            quote.Order();
            this.Transaction.Derive(false);

            Assert.Equal(new SerialisedItemAvailabilities(this.Transaction).InRent, quote.SalesOrderWhereQuote.SalesOrderItems.First().NextSerialisedItemAvailability);
        }
    }
}
