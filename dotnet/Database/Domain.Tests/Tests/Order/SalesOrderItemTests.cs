namespace Allors.Database.Domain.Tests
{
    using Allors.Database.Derivations;
    using System.Collections.Generic;
    using Xunit;

    public class SalesOrderItemTest : DomainTest, IClassFixture<Fixture>
    {
        public SalesOrderItemTest(Fixture fixture) : base(fixture) { }

        [Fact]
        public void OnPostDeriveAssertRentalType()
        {
            var orderItem = new SalesOrderItemBuilder(this.Transaction)
                .WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductItem)
                .WithSaleKind(new SaleKinds(this.Transaction).Rental)
                .WithSerialisedItem(new SerialisedItemBuilder(this.Transaction).Build())
                .WithNextSerialisedItemAvailability(new SerialisedItemAvailabilities(this.Transaction).Sold)
                .WithQuantityOrdered(1)
                .Build();

            var errors = new List<IDerivationError>(this.Transaction.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.Equals("SalesOrderItem.RentalType is required"));
        }

        [Fact]
        public void OnPostDeriveAssertSaleKind()
        {
            var orderItem = new SalesOrderItemBuilder(this.Transaction)
                .WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductItem)
                .WithSerialisedItem(new SerialisedItemBuilder(this.Transaction).Build())
                .WithNextSerialisedItemAvailability(new SerialisedItemAvailabilities(this.Transaction).Sold)
                .WithQuantityOrdered(1)
                .Build();

            var errors = new List<IDerivationError>(this.Transaction.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.Equals("SalesOrderItem.SaleKind is required"));
        }

        [Fact]
        public void ChangedSerialisedItemDeriveDescription()
        {
            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();

            var order = new SalesOrderBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var orderItem = new SalesOrderItemBuilder(this.Transaction).WithSerialisedItem(serialisedItem).Build();
            order.AddSalesOrderItem(orderItem);
            this.Transaction.Derive(false);

            Assert.True(orderItem.ExistDescription);
        }

        [Fact]
        public void ChangedProductDeriveDescription()
        {
            var product = new UnifiedGoodBuilder(this.Transaction).Build();

            var order = new SalesOrderBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var orderItem = new SalesOrderItemBuilder(this.Transaction).WithProduct(product).Build();
            order.AddSalesOrderItem(orderItem);
            this.Transaction.Derive(false);

            Assert.True(orderItem.ExistDescription);
        }
    }
}
