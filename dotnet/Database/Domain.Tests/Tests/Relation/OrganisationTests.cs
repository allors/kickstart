// <copyright file="PurchaseInvoiceTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

using Allors.Database.Meta;
using System.Linq;
using Xunit;
using Allors.Database.Domain.TestPopulation;

namespace Allors.Database.Domain.Tests
{
    public class OrganisationCalculationRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public OrganisationCalculationRuleTests(Fixture fixture) : base(fixture) 
        {
            this.Customer = new OrganisationBuilder(this.Transaction).WithName("customer").Build();
        }

        public Database.Domain.Organisation Customer { get; }

        [Fact]
        public void ChangedCleaningCalculationThrowExpressionError()
        {
            this.Customer.CleaningCalculation = "1 * 1M";

            var errors = this.Derive().Errors.ToList();
            Assert.Equal(new IRoleType[]
            {
                this.M.Organisation.CleaningCalculation,
            }, errors.SelectMany(v => v.RoleTypes).Distinct());
        }

        [Fact]
        public void ChangedCleaningCalculationThrowNotImplementedError()
        {
            this.Customer.CleaningCalculation = "[2]";

            var errors = this.Derive().Errors.ToList();
            Assert.Contains(errors, e => e.Message.Contains("not implemented:"));
        }

        [Fact]
        public void ChangedSundriesCalculationThrowExpressionError()
        {
            this.Customer.SundriesCalculation = "1 * 1M";

            var errors = this.Derive().Errors.ToList();
            Assert.Equal(new IRoleType[]
            {
                this.M.Organisation.SundriesCalculation,
            }, errors.SelectMany(v => v.RoleTypes).Distinct());
        }

        [Fact]
        public void ChangedSundriesCalculationThrowNotImplementedError()
        {
            this.Customer.SundriesCalculation = "[2]";

            var errors = this.Derive().Errors.ToList();
            Assert.Contains(errors, e => e.Message.Contains("not implemented:"));
        }
    }

    public class InternalOrganisationSparePartsRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public InternalOrganisationSparePartsRuleTests(Fixture fixture) : base(fixture)
        {
        }

        [Fact]
        public void ChangedPartDefaultFacilityDeriveInternalOrganisationSpareParts()
        {
            var part = new NonUnifiedPartBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            Assert.DoesNotContain(part, this.InternalOrganisation.SpareParts);

            part.DefaultFacility = this.DefaultFacility;
            this.Transaction.Derive(false);

            Assert.Contains(part, this.InternalOrganisation.SpareParts);
        }

        [Fact]
        public void ChangedFacilityOwnerDeriveInternalOrganisationSpareParts()
        {
            var part = new NonUnifiedPartBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            Assert.DoesNotContain(part, this.InternalOrganisation.SpareParts);

            part.DefaultFacility.Owner = this.InternalOrganisation;
            this.Transaction.Derive(false);

            Assert.Contains(part, this.InternalOrganisation.SpareParts);
        }
    }

    public class InternalOrganisationSerialisedItemsRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public InternalOrganisationSerialisedItemsRuleTests(Fixture fixture) : base(fixture)
        {
        }

        [Fact]
        public void ChangedSerialisedItemOwnedByDeriveInternalOrganisationSerialisedItems()
        {
            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            Assert.DoesNotContain(serialisedItem, this.InternalOrganisation.SerialisedItems);

            serialisedItem.OwnedBy = this.InternalOrganisation;
            this.Transaction.Derive(false);

            Assert.Contains(serialisedItem, this.InternalOrganisation.SerialisedItems);
        }

        [Fact]
        public void ChangedSerialisedItemRentedByDeriveInternalOrganisationSerialisedItems()
        {
            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            Assert.DoesNotContain(serialisedItem, this.InternalOrganisation.SerialisedItems);

            serialisedItem.RentedBy = this.InternalOrganisation;
            this.Transaction.Derive(false);

            Assert.Contains(serialisedItem, this.InternalOrganisation.SerialisedItems);
        }

        [Fact]
        public void ChangedSerialisedItemBuyerDeriveInternalOrganisationSerialisedItems()
        {
            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            Assert.DoesNotContain(serialisedItem, this.InternalOrganisation.SerialisedItems);

            serialisedItem.Buyer = this.InternalOrganisation;
            this.Transaction.Derive(false);

            Assert.Contains(serialisedItem, this.InternalOrganisation.SerialisedItems);
        }

        [Fact]
        public void ChangedSerialisedItemSellerDeriveInternalOrganisationSerialisedItems()
        {
            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            Assert.DoesNotContain(serialisedItem, this.InternalOrganisation.SerialisedItems);

            serialisedItem.Seller = this.InternalOrganisation;
            this.Transaction.Derive(false);

            Assert.Contains(serialisedItem, this.InternalOrganisation.SerialisedItems);
        }

        [Fact]
        public void ChangedRequestItemSerialisedItemDeriveInternalOrganisationSerialisedItems()
        {
            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            Assert.DoesNotContain(serialisedItem, this.InternalOrganisation.SerialisedItems);

            new RequestForQuoteBuilder(this.Transaction)
                .WithRecipient(this.InternalOrganisation)
                .WithRequestItem(new RequestItemBuilder(this.Transaction).WithSerialisedItem(serialisedItem).Build())
                .Build();

            this.Transaction.Derive(false);

            Assert.Contains(serialisedItem, this.InternalOrganisation.SerialisedItems);
        }

        [Fact]
        public void ChangedQuoteItemSerialisedItemDeriveInternalOrganisationSerialisedItems()
        {
            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            Assert.DoesNotContain(serialisedItem, this.InternalOrganisation.SerialisedItems);

            new ProductQuoteBuilder(this.Transaction)
                .WithIssuer(this.InternalOrganisation)
                .WithQuoteItem(new QuoteItemBuilder(this.Transaction).WithSerialisedItem(serialisedItem).Build())
                .Build();

            this.Transaction.Derive(false);

            Assert.Contains(serialisedItem, this.InternalOrganisation.SerialisedItems);
        }

        [Fact]
        public void ChangedPurchaseOrderItemSerialisedItemDeriveInternalOrganisationSerialisedItems()
        {
            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            Assert.DoesNotContain(serialisedItem, this.InternalOrganisation.SerialisedItems);

            new PurchaseOrderBuilder(this.Transaction)
                .WithOrderedBy(this.InternalOrganisation)
                .WithPurchaseOrderItem(new PurchaseOrderItemBuilder(this.Transaction).WithSerialisedItem(serialisedItem).Build())
                .Build();

            this.Transaction.Derive(false);

            Assert.Contains(serialisedItem, this.InternalOrganisation.SerialisedItems);
        }

        [Fact]
        public void ChangedPurchaseInvoiceItemSerialisedItemDeriveInternalOrganisationSerialisedItems()
        {
            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            Assert.DoesNotContain(serialisedItem, this.InternalOrganisation.SerialisedItems);

            new PurchaseInvoiceBuilder(this.Transaction)
                .WithBilledTo(this.InternalOrganisation)
                .WithPurchaseInvoiceItem(new PurchaseInvoiceItemBuilder(this.Transaction).WithSerialisedItem(serialisedItem).WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).PartItem).Build())
                .Build();

            this.Transaction.Derive(false);

            Assert.Contains(serialisedItem, this.InternalOrganisation.SerialisedItems);
        }

        [Fact]
        public void ChangedSalesOrderItemSerialisedItemDeriveInternalOrganisationSerialisedItems()
        {
            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            Assert.DoesNotContain(serialisedItem, this.InternalOrganisation.SerialisedItems);

            new SalesOrderBuilder(this.Transaction)
                .WithTakenBy(this.InternalOrganisation)
                .WithSalesOrderItem(new SalesOrderItemBuilder(this.Transaction).WithSerialisedItem(serialisedItem).Build())
                .Build();

            this.Transaction.Derive(false);

            Assert.Contains(serialisedItem, this.InternalOrganisation.SerialisedItems);
        }

        [Fact]
        public void ChangedSalesInvoiceItemSerialisedItemDeriveInternalOrganisationSerialisedItems()
        {
            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            Assert.DoesNotContain(serialisedItem, this.InternalOrganisation.SerialisedItems);

            new SalesInvoiceBuilder(this.Transaction)
                .WithBilledFrom(this.InternalOrganisation)
                .WithSalesInvoiceItem(new SalesInvoiceItemBuilder(this.Transaction).WithSerialisedItem(serialisedItem).Build())
                .Build();

            this.Transaction.Derive(false);

            Assert.Contains(serialisedItem, this.InternalOrganisation.SerialisedItems);
        }

        [Fact]
        public void ChangedCustomerShipmentItemSerialisedItemDeriveInternalOrganisationSerialisedItems()
        {
            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            Assert.DoesNotContain(serialisedItem, this.InternalOrganisation.SerialisedItems);

            new CustomerShipmentBuilder(this.Transaction)
                .WithShipFromParty(this.InternalOrganisation)
                .WithShipmentItem(new ShipmentItemBuilder(this.Transaction).WithSerialisedItem(serialisedItem).Build())
                .Build();

            this.Transaction.Derive(false);

            Assert.Contains(serialisedItem, this.InternalOrganisation.SerialisedItems);
        }

        [Fact]
        public void ChangedPurchaseShipmentItemSerialisedItemDeriveInternalOrganisationSerialisedItems()
        {
            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            Assert.DoesNotContain(serialisedItem, this.InternalOrganisation.SerialisedItems);

            new PurchaseShipmentBuilder(this.Transaction)
                .WithShipToParty(this.InternalOrganisation)
                .WithShipmentItem(new ShipmentItemBuilder(this.Transaction).WithSerialisedItem(serialisedItem).Build())
                .Build();

            this.Transaction.Derive(false);

            Assert.Contains(serialisedItem, this.InternalOrganisation.SerialisedItems);
        }
    }
}

