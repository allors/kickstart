namespace Allors.Database.Domain.Tests
{
    using Allors.Database.Derivations;
    using System.Collections.Generic;
    using TestPopulation;
    using Xunit;

    public class QuoteItemTest : DomainTest, IClassFixture<Fixture>
    {
        public QuoteItemTest(Fixture fixture) : base(fixture) { }

        [Fact]
        public void OnPostDeriveAssertRentalType()
        {
            var quoteItem = new QuoteItemBuilder(this.Transaction)
                .WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductItem)
                .WithSaleKind(new SaleKinds(this.Transaction).Rental)
                .WithSerialisedItem(new SerialisedItemBuilder(this.Transaction).Build())
                .WithQuantity(1)
                .Build();

            var errors = new List<IDerivationError>(this.Transaction.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.Equals("QuoteItem.RentalType is required"));
        }

        [Fact]
        public void OnPostDeriveAssertSaleKind()
        {
            var quoteItem = new QuoteItemBuilder(this.Transaction)
                .WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductItem)
                .WithSerialisedItem(new SerialisedItemBuilder(this.Transaction).Build())
                .WithQuantity(1)
                .Build();

            var errors = new List<IDerivationError>(this.Transaction.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.Equals("QuoteItem.SaleKind is required"));
        }

        [Fact]
        public void ChangedSparePartDescriptionOrProductDeriveAssertAtleastOneProductOrSparePartDescription()
        {
            var quoteItem = new QuoteItemBuilder(this.Transaction)
                .WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).PartItem)
                .WithQuantity(2)
                .Build();

            var errors = new List<IDerivationError>(this.Transaction.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.Equals("QuoteItem.Product, QuoteItem.SparePartDescription at least one"));

            quoteItem.SparePartDescription = "SparePartDescription";

            Assert.False(this.Derive().HasErrors);

            quoteItem.Product = new NonUnifiedPartBuilder(this.Transaction).WithNonSerialisedDefaults(this.InternalOrganisation).Build();
            quoteItem.RemoveSparePartDescription();

            Assert.False(this.Derive().HasErrors);
        }

        [Fact]
        public void ChangedSparePartDescriptionOrProductDeriveAssertExistAtMostOneProductOrSparePartDescription()
        {
            var quoteItem = new QuoteItemBuilder(this.Transaction)
                .WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).PartItem)
                .WithSparePartDescription("SparePartDescription")
                .WithProduct(new NonUnifiedPartBuilder(this.Transaction).WithNonSerialisedDefaults(this.InternalOrganisation).Build())
                .WithQuantity(2)
                .Build();

            var errors = new List<IDerivationError>(this.Transaction.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.Equals("QuoteItem.Product, QuoteItem.SparePartDescription at most one"));
        }

        [Fact]
        public void ChangedSerialisedItemDeriveDetails()
        {
            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();

            var quote = new ProductQuoteBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var quoteItem = new QuoteItemBuilder(this.Transaction).WithSerialisedItem(serialisedItem).Build();
            quote.AddQuoteItem(quoteItem);
            this.Transaction.Derive(false);

            Assert.True(quoteItem.ExistDetails);
        }

        [Fact]
        public void ChangedProductDeriveDetails()
        {
            var product = new UnifiedGoodBuilder(this.Transaction).Build();

            var quote = new ProductQuoteBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var quoteItem = new QuoteItemBuilder(this.Transaction).WithProduct(product).Build();
            quote.AddQuoteItem(quoteItem);
            this.Transaction.Derive(false);

            Assert.True(quoteItem.ExistDetails);
        }
    }
}
