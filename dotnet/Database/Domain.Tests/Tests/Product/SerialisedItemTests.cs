// <copyright file="SerialisedItemTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

using Allors.Database.Domain.TestPopulation;
using Resources;
using System.Linq;
using Xunit;

namespace Allors.Database.Domain.Tests
{
    public class SerialisedItemCustomRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public SerialisedItemCustomRuleTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedSerialisedInventoryItemSerialisedItemDeriveLocation()
        {
            var part = new UnifiedGoodBuilder(this.Transaction).WithInventoryItemKind(new InventoryItemKinds(this.Transaction).Serialised).Build();
            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            part.AddSerialisedItem(serialisedItem);
            this.Transaction.Derive(false);

            new InventoryItemTransactionBuilder(this.Transaction)
                .WithPart(part)
                .WithSerialisedItem(serialisedItem)
                .WithReason(new InventoryTransactionReasons(this.Transaction).IncomingShipment)
                .WithFacility(this.InternalOrganisation.FacilitiesWhereOwner.First())
                .WithQuantity(1)
                .Build();
            this.Transaction.Derive(false);

            Assert.Equal(this.InternalOrganisation.FacilitiesWhereOwner.First().Name, serialisedItem.Location);
        }

        [Fact]
        public void ChangedSerialisedInventoryItemQuantityDeriveLocation()
        {
            var part = new UnifiedGoodBuilder(this.Transaction).WithInventoryItemKind(new InventoryItemKinds(this.Transaction).Serialised).Build();
            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            part.AddSerialisedItem(serialisedItem);
            this.Transaction.Derive(false);

            new InventoryItemTransactionBuilder(this.Transaction)
                .WithPart(part)
                .WithSerialisedItem(serialisedItem)
                .WithReason(new InventoryTransactionReasons(this.Transaction).IncomingShipment)
                .WithFacility(this.InternalOrganisation.FacilitiesWhereOwner.First())
                .WithQuantity(1)
                .Build();
            this.Transaction.Derive(false);

            new InventoryItemTransactionBuilder(this.Transaction)
                .WithPart(part)
                .WithSerialisedItem(serialisedItem)
                .WithReason(new InventoryTransactionReasons(this.Transaction).OutgoingShipment)
                .WithFacility(this.InternalOrganisation.FacilitiesWhereOwner.First())
                .WithQuantity(1)
                .Build();
            this.Transaction.Derive(false);

            Assert.False(serialisedItem.ExistLocation);
        }

        [Fact]
        public void ChangedFacilityNameDeriveLocation()
        {
            var part = new UnifiedGoodBuilder(this.Transaction).WithInventoryItemKind(new InventoryItemKinds(this.Transaction).Serialised).Build();
            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            part.AddSerialisedItem(serialisedItem);
            this.Transaction.Derive(false);

            new InventoryItemTransactionBuilder(this.Transaction)
                .WithPart(part)
                .WithSerialisedItem(serialisedItem)
                .WithReason(new InventoryTransactionReasons(this.Transaction).IncomingShipment)
                .WithFacility(this.InternalOrganisation.FacilitiesWhereOwner.First())
                .WithQuantity(1)
                .Build();
            this.Transaction.Derive(false);

            this.InternalOrganisation.FacilitiesWhereOwner.First().Name = "changed";
            this.Transaction.Derive(false);

            Assert.Equal("changed", serialisedItem.Location);
        }

        [Fact]
        public void ChangedUnifiedGoodIataGseCodeDeriveIataCode()
        {
            var iataGseCode = this.Transaction.Extent<IataGseCode>().First();
            var part = new UnifiedGoodBuilder(this.Transaction).WithInventoryItemKind(new InventoryItemKinds(this.Transaction).Serialised).Build();
            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            part.AddSerialisedItem(serialisedItem);
            this.Transaction.Derive(false);

            part.IataGseCode = iataGseCode;
            this.Transaction.Derive(false);

            Assert.Equal(iataGseCode.Code, serialisedItem.IataCode);
        }

        [Fact]
        public void ChangedIataGseCodeCodeDeriveIataCode()
        {
            var iataGseCode = this.Transaction.Extent<IataGseCode>().First();
            var part = new UnifiedGoodBuilder(this.Transaction).WithInventoryItemKind(new InventoryItemKinds(this.Transaction).Serialised).WithIataGseCode(iataGseCode).Build();
            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            part.AddSerialisedItem(serialisedItem);
            this.Transaction.Derive(false);

            iataGseCode.Code = "changed";
            this.Transaction.Derive(false);

            Assert.Equal("changed", serialisedItem.IataCode);
        }

        [Fact]
        public void ChangedSalesInvoiceItemSerialisedItemDeriveSellingPrice()
        {
            var serialisedItem = new SerialisedItemBuilder(this.Transaction)
                .WithSerialisedItemAvailability(new SerialisedItemAvailabilities(this.Transaction).Sold)
                .WithOwnership(new Ownerships(this.Transaction).ThirdParty)
                .Build();

            var invoice = new SalesInvoiceBuilder(this.Transaction).Build();
            var invoiceItem = new SalesInvoiceItemBuilder(this.Transaction)
                .WithSerialisedItem(serialisedItem)
                .WithTotalExVat(10)
                .Build();
            invoice.AddSalesInvoiceItem(invoiceItem);
            this.Transaction.Derive(false);

            Assert.Equal(10, serialisedItem.SellingPrice);
        }

        [Fact]
        public void ChangedSerialisedItemSerialisedItemAvailabilityDeriveSellingPrice()
        {
            var serialisedItem = new SerialisedItemBuilder(this.Transaction)
                .WithSerialisedItemAvailability(new SerialisedItemAvailabilities(this.Transaction).Available)
                .WithOwnership(new Ownerships(this.Transaction).ThirdParty)
                .Build();

            var invoice = new SalesInvoiceBuilder(this.Transaction).Build();
            var invoiceItem = new SalesInvoiceItemBuilder(this.Transaction)
                .WithSerialisedItem(serialisedItem)
                .WithTotalExVat(10)
                .Build();
            invoice.AddSalesInvoiceItem(invoiceItem);
            this.Transaction.Derive(false);

            serialisedItem.SerialisedItemAvailability = new SerialisedItemAvailabilities(this.Transaction).Sold;
            this.Transaction.Derive(false);

            Assert.Equal(10, serialisedItem.SellingPrice);
        }

        [Fact]
        public void ChangedSerialisedItemOwnershipDeriveSellingPrice()
        {
            var serialisedItem = new SerialisedItemBuilder(this.Transaction)
                .WithSerialisedItemAvailability(new SerialisedItemAvailabilities(this.Transaction).Sold)
                .WithOwnership(new Ownerships(this.Transaction).Own)
                .Build();

            var invoice = new SalesInvoiceBuilder(this.Transaction).Build();
            var invoiceItem = new SalesInvoiceItemBuilder(this.Transaction)
                .WithSerialisedItem(serialisedItem)
                .WithTotalExVat(10)
                .Build();
            invoice.AddSalesInvoiceItem(invoiceItem);
            this.Transaction.Derive(false);

            serialisedItem.Ownership = new Ownerships(this.Transaction).ThirdParty;
            this.Transaction.Derive(false);

            Assert.Equal(10, serialisedItem.SellingPrice);
        }

        [Fact]
        public void ChangedSalesInvoiceItemTotalExVatDeriveSellingPrice()
        {
            var serialisedItem = new SerialisedItemBuilder(this.Transaction)
                .WithSerialisedItemAvailability(new SerialisedItemAvailabilities(this.Transaction).Sold)
                .WithOwnership(new Ownerships(this.Transaction).ThirdParty)
                .Build();

            var invoice = new SalesInvoiceBuilder(this.Transaction).Build();
            var invoiceItem = new SalesInvoiceItemBuilder(this.Transaction)
                .WithSerialisedItem(serialisedItem)
                .WithTotalExVat(10)
                .Build();
            invoice.AddSalesInvoiceItem(invoiceItem);
            this.Transaction.Derive(false);

            invoiceItem.TotalExVat = 11;
            this.Transaction.Derive(false);

            Assert.Equal(11, serialisedItem.SellingPrice);
        }
    }

    public class SerialisedItemCostRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public SerialisedItemCostRuleTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedWorkEffortFixedAssetAssignmentFixedAssetDeriveActualRefurbishCost()
        {
            var serialisedItem = new SerialisedItemBuilder(this.Transaction)
                .WithOwnership(new Ownerships(this.Transaction).Own)
                .Build();
            this.Transaction.Derive(false);

            var assignment = new WorkTaskBuilder(this.Transaction)
                .WithWorkEffortState(new WorkEffortStates(this.Transaction).Completed)
                .WithExecutedBy(this.InternalOrganisation)
                .WithCustomer(this.InternalOrganisation)
                .WithTotalCost(10)
                .Build();
            this.Transaction.Derive(false);

            var workEffortFixedAssetAssignment = new WorkEffortFixedAssetAssignmentBuilder(this.Transaction)
                .WithAssignment(assignment)
                .Build();
            this.Transaction.Derive(false);

            workEffortFixedAssetAssignment.FixedAsset = serialisedItem;
            this.Transaction.Derive(false);

            Assert.Equal(10, serialisedItem.ActualRefurbishCost);
        }

        [Fact]
        public void ChangedWorkEffortWorkEffortStateDeriveActualRefurbishCost()
        {
            var serialisedItem = new SerialisedItemBuilder(this.Transaction)
                .WithOwnership(new Ownerships(this.Transaction).Own)
                .Build();
            this.Transaction.Derive(false);

            var assignment = new WorkTaskBuilder(this.Transaction)
                .WithWorkEffortState(new WorkEffortStates(this.Transaction).Created)
                .WithExecutedBy(this.InternalOrganisation)
                .WithCustomer(this.InternalOrganisation)
                .WithTotalCost(10)
                .Build();
            this.Transaction.Derive(false);

            new WorkEffortFixedAssetAssignmentBuilder(this.Transaction)
                .WithAssignment(assignment)
                .WithFixedAsset(serialisedItem)
                .Build();
            this.Transaction.Derive(false);

            assignment.WorkEffortState = new WorkEffortStates(this.Transaction).Completed;
            this.Transaction.Derive(false);

            Assert.Equal(10, serialisedItem.ActualRefurbishCost);
        }

        [Fact]
        public void ChangedWorkEffortCustomerDeriveActualRefurbishCost()
        {
            var serialisedItem = new SerialisedItemBuilder(this.Transaction)
                .WithOwnership(new Ownerships(this.Transaction).Own)
                .Build();
            this.Transaction.Derive(false);

            var assignment = new WorkTaskBuilder(this.Transaction)
                .WithWorkEffortState(new WorkEffortStates(this.Transaction).Completed)
                .WithExecutedBy(this.InternalOrganisation)
                .WithTotalCost(10)
                .Build();
            this.Transaction.Derive(false);

            new WorkEffortFixedAssetAssignmentBuilder(this.Transaction)
                .WithAssignment(assignment)
                .WithFixedAsset(serialisedItem)
                .Build();
            this.Transaction.Derive(false);

            assignment.Customer = this.InternalOrganisation;
            this.Transaction.Derive(false);

            Assert.Equal(10, serialisedItem.ActualRefurbishCost);
        }

        [Fact]
        public void ChangedWorkEffortExecutedByDeriveActualRefurbishCost()
        {
            var serialisedItem = new SerialisedItemBuilder(this.Transaction)
                .WithOwnership(new Ownerships(this.Transaction).Own)
                .Build();
            this.Transaction.Derive(false);

            var assignment = new WorkTaskBuilder(this.Transaction)
                .WithWorkEffortState(new WorkEffortStates(this.Transaction).Completed)
                .WithCustomer(this.InternalOrganisation)
                .WithTotalCost(10)
                .Build();
            this.Transaction.Derive(false);

            new WorkEffortFixedAssetAssignmentBuilder(this.Transaction)
                .WithAssignment(assignment)
                .WithFixedAsset(serialisedItem)
                .Build();
            this.Transaction.Derive(false);

            assignment.ExecutedBy = this.InternalOrganisation;
            this.Transaction.Derive(false);

            Assert.Equal(10, serialisedItem.ActualRefurbishCost);
        }

        [Fact]
        public void ChangedOwnershipDeriveActualRefurbishCost()
        {
            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var assignment = new WorkTaskBuilder(this.Transaction)
                .WithWorkEffortState(new WorkEffortStates(this.Transaction).Completed)
                .WithCustomer(this.InternalOrganisation)
                .WithExecutedBy(this.InternalOrganisation)
                .WithTotalCost(10)
                .Build();
            this.Transaction.Derive(false);

            new WorkEffortFixedAssetAssignmentBuilder(this.Transaction)
                .WithAssignment(assignment)
                .WithFixedAsset(serialisedItem)
                .Build();
            this.Transaction.Derive(false);

            serialisedItem.Ownership = new Ownerships(this.Transaction).Own;
            this.Transaction.Derive(false);

            Assert.Equal(10, serialisedItem.ActualRefurbishCost);
        }

        [Fact]
        public void ChangedWorkEffortTotalCostDeriveActualRefurbishCost()
        {
            var serialisedItem = new SerialisedItemBuilder(this.Transaction)
                .WithOwnership(new Ownerships(this.Transaction).Own)
                .Build();
            this.Transaction.Derive(false);

            var assignment = new WorkTaskBuilder(this.Transaction)
                .WithWorkEffortState(new WorkEffortStates(this.Transaction).Completed)
                .WithCustomer(this.InternalOrganisation)
                .WithExecutedBy(this.InternalOrganisation)
                .WithTotalCost(10)
                .Build();
            this.Transaction.Derive(false);

            new WorkEffortFixedAssetAssignmentBuilder(this.Transaction)
                .WithAssignment(assignment)
                .WithFixedAsset(serialisedItem)
                .Build();
            this.Transaction.Derive(false);

            assignment.TotalCost = 11;
            this.Transaction.Derive(false);

            Assert.Equal(11, serialisedItem.ActualRefurbishCost);
        }

        [Fact]
        public void ChangedWorkEffortExecutedByDeriveActualRefurbishCost_2()
        {
            var serialisedItem = new SerialisedItemBuilder(this.Transaction)
                .WithOwnership(new Ownerships(this.Transaction).Own)
                .Build();
            this.Transaction.Derive(false);

            var assignment = new WorkTaskBuilder(this.Transaction)
                .WithWorkEffortState(new WorkEffortStates(this.Transaction).Completed)
                .WithCustomer(this.InternalOrganisation)
                .Build();
            this.Transaction.Derive(false);

            assignment.WorkEffortInvoiceItemAssignmentsWhereAssignment.First().WorkEffortInvoiceItem.Amount = 10;
            this.Transaction.Derive(false);

            new WorkEffortFixedAssetAssignmentBuilder(this.Transaction)
                .WithAssignment(assignment)
                .WithFixedAsset(serialisedItem)
                .Build();
            this.Transaction.Derive(false);

            assignment.ExecutedBy = new OrganisationBuilder(this.Transaction).WithIsInternalOrganisation(true).Build();
            this.Transaction.Derive(false);

            Assert.Equal(10, serialisedItem.ActualRefurbishCost);
        }

        [Fact]
        public void ChangedWorkEffortTotalRevenueDeriveActualRefurbishCost()
        {
            var serialisedItem = new SerialisedItemBuilder(this.Transaction)
                .WithOwnership(new Ownerships(this.Transaction).Own)
                .Build();
            this.Transaction.Derive(false);

            var assignment = new WorkTaskBuilder(this.Transaction)
                .WithWorkEffortState(new WorkEffortStates(this.Transaction).Completed)
                .WithExecutedBy(this.InternalOrganisation)
                .WithCustomer(new OrganisationBuilder(this.Transaction).WithIsInternalOrganisation(true).Build())
                .WithTotalRevenue(10)
                .Build();
            this.Transaction.Derive(false);

            new WorkEffortFixedAssetAssignmentBuilder(this.Transaction)
                .WithAssignment(assignment)
                .WithFixedAsset(serialisedItem)
                .Build();
            this.Transaction.Derive(false);

            assignment.TotalRevenue = 11;
            this.Transaction.Derive(false);

            Assert.Equal(11, serialisedItem.ActualRefurbishCost);
        }

        [Fact]
        public void ChangedPurchaseInvoiceItemSerialisedItemDeriveActualRefurbishCost()
        {
            var serialisedItem = new SerialisedItemBuilder(this.Transaction)
                .WithBuyer(this.InternalOrganisation)
                .Build();
            this.Transaction.Derive(false);

            var invoice = new PurchaseInvoiceBuilder(this.Transaction)
                .WithPurchaseInvoiceType(new PurchaseInvoiceTypes(Transaction).PurchaseInvoice)
                .WithBilledTo(this.InternalOrganisation)
                .Build();

            var invoiceItem = new PurchaseInvoiceItemBuilder(this.Transaction)
                .WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).Rm)
                .WithQuantity(1)
                .WithAssignedUnitPrice(10)
                .Build();

            invoice.AddPurchaseInvoiceItem(invoiceItem);
            this.Transaction.Derive(false);

            invoiceItem.SerialisedItem = serialisedItem;
            this.Transaction.Derive(false);

            Assert.Equal(10, serialisedItem.ActualRefurbishCost);
        }

        [Fact]
        public void ChangedPurchaseInvoiceItemInvoiceItemTypeDeriveActualRefurbishCost()
        {
            var serialisedItem = new SerialisedItemBuilder(this.Transaction)
                .WithBuyer(this.InternalOrganisation)
                .Build();
            this.Transaction.Derive(false);

            var invoice = new PurchaseInvoiceBuilder(this.Transaction)
                .WithPurchaseInvoiceType(new PurchaseInvoiceTypes(Transaction).PurchaseInvoice)
                .WithBilledTo(this.InternalOrganisation)
                .Build();

            var invoiceItem = new PurchaseInvoiceItemBuilder(this.Transaction)
                .WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductItem)
                .WithSerialisedItem(serialisedItem)
                .WithQuantity(1)
                .WithAssignedUnitPrice(10)
                .Build();

            invoice.AddPurchaseInvoiceItem(invoiceItem);
            this.Transaction.Derive(false);

            invoiceItem.InvoiceItemType = new InvoiceItemTypes(this.Transaction).Rm;
            this.Transaction.Derive(false);

            Assert.Equal(10, serialisedItem.ActualRefurbishCost);
        }

        [Fact]
        public void ChangedPurchaseInvoiceBilledToDeriveActualRefurbishCost()
        {
            var serialisedItem = new SerialisedItemBuilder(this.Transaction)
                .WithBuyer(this.InternalOrganisation)
                .Build();
            this.Transaction.Derive(false);

            var invoice = new PurchaseInvoiceBuilder(this.Transaction).WithPurchaseInvoiceType(new PurchaseInvoiceTypes(Transaction).PurchaseInvoice).Build();
            var invoiceItem = new PurchaseInvoiceItemBuilder(this.Transaction)
                .WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).Rm)
                .WithSerialisedItem(serialisedItem)
                .WithQuantity(1)
                .WithAssignedUnitPrice(10)
                .Build();

            invoice.AddPurchaseInvoiceItem(invoiceItem);
            this.Transaction.Derive(false);

            invoice.BilledTo = this.InternalOrganisation;
            this.Transaction.Derive(false);

            Assert.Equal(10, serialisedItem.ActualRefurbishCost);
        }

        [Fact]
        public void ChangedBuyerDeriveActualRefurbishCost()
        {
            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var invoice = new PurchaseInvoiceBuilder(this.Transaction)
                .WithPurchaseInvoiceType(new PurchaseInvoiceTypes(Transaction).PurchaseInvoice)
                .WithBilledTo(this.InternalOrganisation)
                .Build();

            var invoiceItem = new PurchaseInvoiceItemBuilder(this.Transaction)
                .WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).Rm)
                .WithSerialisedItem(serialisedItem)
                .WithQuantity(1)
                .WithAssignedUnitPrice(10)
                .Build();

            invoice.AddPurchaseInvoiceItem(invoiceItem);
            this.Transaction.Derive(false);

            serialisedItem.Buyer = this.InternalOrganisation;
            this.Transaction.Derive(false);

            Assert.Equal(10, serialisedItem.ActualRefurbishCost);
        }

        [Fact]
        public void ChangedSellerDeriveActualRefurbishCost()
        {
            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var invoice = new PurchaseInvoiceBuilder(this.Transaction)
                .WithPurchaseInvoiceType(new PurchaseInvoiceTypes(Transaction).PurchaseInvoice)
                .WithBilledTo(this.InternalOrganisation)
                .Build();

            var invoiceItem = new PurchaseInvoiceItemBuilder(this.Transaction)
                .WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).Rm)
                .WithSerialisedItem(serialisedItem)
                .WithQuantity(1)
                .WithAssignedUnitPrice(10)
                .Build();

            invoice.AddPurchaseInvoiceItem(invoiceItem);
            this.Transaction.Derive(false);

            serialisedItem.Seller = this.InternalOrganisation;
            this.Transaction.Derive(false);

            Assert.Equal(10, serialisedItem.ActualRefurbishCost);
        }

        [Fact]
        public void ChangedPurchaseInvoiceItemTotalExVatDeriveActualRefurbishCost()
        {
            var serialisedItem = new SerialisedItemBuilder(this.Transaction)
                .WithBuyer(this.InternalOrganisation)
                .Build();
            this.Transaction.Derive(false);

            var invoice = new PurchaseInvoiceBuilder(this.Transaction)
                .WithPurchaseInvoiceType(new PurchaseInvoiceTypes(Transaction).PurchaseInvoice)
                .WithBilledTo(this.InternalOrganisation)
                .Build();

            var invoiceItem = new PurchaseInvoiceItemBuilder(this.Transaction)
                .WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).Rm)
                .WithSerialisedItem(serialisedItem)
                .WithQuantity(1)
                .WithAssignedUnitPrice(10)
                .Build();

            invoice.AddPurchaseInvoiceItem(invoiceItem);
            this.Transaction.Derive(false);

            invoiceItem.AssignedUnitPrice = 11;
            this.Transaction.Derive(false);

            Assert.Equal(11, serialisedItem.ActualRefurbishCost);
        }

        [Fact]
        public void ChangedPurchaseInvoiceItemSerialisedItemDeriveActualTransportCost()
        {
            var serialisedItem = new SerialisedItemBuilder(this.Transaction)
                .WithBuyer(this.InternalOrganisation)
                .Build();
            this.Transaction.Derive(false);

            var invoice = new PurchaseInvoiceBuilder(this.Transaction)
                .WithPurchaseInvoiceType(new PurchaseInvoiceTypes(Transaction).PurchaseInvoice)
                .WithBilledTo(this.InternalOrganisation)
                .Build();

            var invoiceItem = new PurchaseInvoiceItemBuilder(this.Transaction)
                .WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).Transport)
                .WithQuantity(1)
                .WithAssignedUnitPrice(10)
                .Build();

            invoice.AddPurchaseInvoiceItem(invoiceItem);
            this.Transaction.Derive(false);

            invoiceItem.SerialisedItem = serialisedItem;
            this.Transaction.Derive(false);

            Assert.Equal(10, serialisedItem.ActualTransportCost);
        }

        [Fact]
        public void ChangedPurchaseInvoiceItemInvoiceItemTypeDeriveActualTransportCost()
        {
            var serialisedItem = new SerialisedItemBuilder(this.Transaction)
                .WithBuyer(this.InternalOrganisation)
                .Build();
            this.Transaction.Derive(false);

            var invoice = new PurchaseInvoiceBuilder(this.Transaction)
                .WithPurchaseInvoiceType(new PurchaseInvoiceTypes(Transaction).PurchaseInvoice)
                .WithBilledTo(this.InternalOrganisation)
                .Build();

            var invoiceItem = new PurchaseInvoiceItemBuilder(this.Transaction)
                .WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).ProductItem)
                .WithSerialisedItem(serialisedItem)
                .WithQuantity(1)
                .WithAssignedUnitPrice(10)
                .Build();

            invoice.AddPurchaseInvoiceItem(invoiceItem);
            this.Transaction.Derive(false);

            invoiceItem.InvoiceItemType = new InvoiceItemTypes(this.Transaction).Transport;
            this.Transaction.Derive(false);

            Assert.Equal(10, serialisedItem.ActualTransportCost);
        }

        [Fact]
        public void ChangedPurchaseInvoiceBilledToDeriveActualTransportCost()
        {
            var serialisedItem = new SerialisedItemBuilder(this.Transaction)
                .WithBuyer(this.InternalOrganisation)
                .Build();
            this.Transaction.Derive(false);

            var invoice = new PurchaseInvoiceBuilder(this.Transaction).WithPurchaseInvoiceType(new PurchaseInvoiceTypes(Transaction).PurchaseInvoice).Build();
            var invoiceItem = new PurchaseInvoiceItemBuilder(this.Transaction)
                .WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).Transport)
                .WithSerialisedItem(serialisedItem)
                .WithQuantity(1)
                .WithAssignedUnitPrice(10)
                .Build();

            invoice.AddPurchaseInvoiceItem(invoiceItem);
            this.Transaction.Derive(false);

            invoice.BilledTo = this.InternalOrganisation;
            this.Transaction.Derive(false);

            Assert.Equal(10, serialisedItem.ActualTransportCost);
        }

        [Fact]
        public void ChangedBuyerDeriveActualTransportCost()
        {
            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var invoice = new PurchaseInvoiceBuilder(this.Transaction)
                .WithPurchaseInvoiceType(new PurchaseInvoiceTypes(Transaction).PurchaseInvoice)
                .WithBilledTo(this.InternalOrganisation)
                .Build();

            var invoiceItem = new PurchaseInvoiceItemBuilder(this.Transaction)
                .WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).Transport)
                .WithSerialisedItem(serialisedItem)
                .WithQuantity(1)
                .WithAssignedUnitPrice(10)
                .Build();

            invoice.AddPurchaseInvoiceItem(invoiceItem);
            this.Transaction.Derive(false);

            serialisedItem.Buyer = this.InternalOrganisation;
            this.Transaction.Derive(false);

            Assert.Equal(10, serialisedItem.ActualTransportCost);
        }

        [Fact]
        public void ChangedSellerDeriveActualTransportCost()
        {
            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var invoice = new PurchaseInvoiceBuilder(this.Transaction)
                .WithPurchaseInvoiceType(new PurchaseInvoiceTypes(Transaction).PurchaseInvoice)
                .WithBilledTo(this.InternalOrganisation)
                .Build();

            var invoiceItem = new PurchaseInvoiceItemBuilder(this.Transaction)
                .WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).Transport)
                .WithSerialisedItem(serialisedItem)
                .WithQuantity(1)
                .WithAssignedUnitPrice(10)
                .Build();

            invoice.AddPurchaseInvoiceItem(invoiceItem);
            this.Transaction.Derive(false);

            serialisedItem.Seller = this.InternalOrganisation;
            this.Transaction.Derive(false);

            Assert.Equal(10, serialisedItem.ActualTransportCost);
        }

        [Fact]
        public void ChangedPurchaseInvoiceItemTotalExVatDeriveActualTransportCost()
        {
            var serialisedItem = new SerialisedItemBuilder(this.Transaction)
                .WithBuyer(this.InternalOrganisation)
                .Build();
            this.Transaction.Derive(false);

            var invoice = new PurchaseInvoiceBuilder(this.Transaction)
                .WithPurchaseInvoiceType(new PurchaseInvoiceTypes(Transaction).PurchaseInvoice)
                .WithBilledTo(this.InternalOrganisation)
                .Build();

            var invoiceItem = new PurchaseInvoiceItemBuilder(this.Transaction)
                .WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).Transport)
                .WithSerialisedItem(serialisedItem)
                .WithQuantity(1)
                .WithAssignedUnitPrice(10)
                .Build();

            invoice.AddPurchaseInvoiceItem(invoiceItem);
            this.Transaction.Derive(false);

            invoiceItem.AssignedUnitPrice = 11;
            this.Transaction.Derive(false);

            Assert.Equal(11, serialisedItem.ActualTransportCost);
        }
    }

    public class SerialisedItemPurchaseInvoiceDerivationTests : DomainTest, IClassFixture<Fixture>
    {
        public SerialisedItemPurchaseInvoiceDerivationTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedPurchaseInvoicePurchaseInvoiceStateDerivePurchaseInvoice()
        {
            var invoice = new PurchaseInvoiceBuilder(this.Transaction).WithBilledTo(this.InternalOrganisation).WithBilledFrom(this.Supplier).WithInvoiceDate(this.Transaction.Now()).Build();
            this.Transaction.Derive(false);

            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            var invoiceItem = new PurchaseInvoiceItemBuilder(this.Transaction)
                .WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).GseUnmotorized)
                .WithSerialisedItem(serialisedItem)
                .Build();
            invoice.AddPurchaseInvoiceItem(invoiceItem);
            this.Transaction.Derive(false);

            invoice.Confirm();
            this.Transaction.Derive(false);

            invoice.Approve();
            this.Transaction.Derive(false);

            Assert.Equal(invoice, serialisedItem.PurchaseInvoice);
        }

        [Fact]
        public void ChangedPurchaseInvoiceValidInvoiceItemsDerivePurchaseInvoice()
        {
            var invoice = new PurchaseInvoiceBuilder(this.Transaction).WithBilledTo(this.InternalOrganisation).WithBilledFrom(this.Supplier).Build();
            this.Transaction.Derive(false);

            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            var invoiceItem = new PurchaseInvoiceItemBuilder(this.Transaction)
                .WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).GseUnmotorized)
                .WithSerialisedItem(serialisedItem)
                .Build();
            invoice.AddPurchaseInvoiceItem(invoiceItem);
            this.Transaction.Derive(false);

            invoice.Confirm();
            this.Transaction.Derive(false);

            invoice.Approve();
            this.Transaction.Derive(false);

            Assert.Equal(invoice, serialisedItem.PurchaseInvoice);

            invoiceItem.CancelFromInvoice();
            this.Transaction.Derive(false);

            Assert.False(serialisedItem.ExistPurchaseInvoice);
        }
    }

    public class SerialisedItemPurchasePriceDerivationTests : DomainTest, IClassFixture<Fixture>
    {
        public SerialisedItemPurchasePriceDerivationTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedPurchaseInvoiceDerivePurchasePrice()
        {
            var invoice = new PurchaseInvoiceBuilder(this.Transaction).WithBilledTo(this.InternalOrganisation).WithBilledFrom(this.Supplier).Build();
            this.Transaction.Derive(false);

            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            var invoiceItem = new PurchaseInvoiceItemBuilder(this.Transaction)
                .WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).GseUnmotorized)
                .WithSerialisedItem(serialisedItem)
                .WithAssignedUnitPrice(1)
                .Build();
            invoice.AddPurchaseInvoiceItem(invoiceItem);
            this.Transaction.Derive(false);

            invoice.Confirm();
            this.Transaction.Derive(false);

            invoice.Approve();
            this.Transaction.Derive(false);

            Assert.Equal(1, serialisedItem.PurchasePrice);
        }

        [Fact]
        public void ChangedAssignedBookValueDeriveDerivedBookValue()
        {
            var invoice = new PurchaseInvoiceBuilder(this.Transaction).WithBilledTo(this.InternalOrganisation).WithBilledFrom(this.Supplier).Build();
            this.Transaction.Derive(false);

            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            var invoiceItem = new PurchaseInvoiceItemBuilder(this.Transaction)
                .WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).GseUnmotorized)
                .WithSerialisedItem(serialisedItem)
                .WithAssignedUnitPrice(1)
                .Build();
            invoice.AddPurchaseInvoiceItem(invoiceItem);
            this.Transaction.Derive(false);

            invoice.Confirm();
            this.Transaction.Derive(false);

            invoice.Approve();
            this.Transaction.Derive(false);

            serialisedItem.AssignedBookValue = 10;
            this.Transaction.Derive(false);

            Assert.Equal(10, serialisedItem.DerivedBookValue);
        }
    }

    public class SerialisedItemPurchaseOrderDerivationTests : DomainTest, IClassFixture<Fixture>
    {
        public SerialisedItemPurchaseOrderDerivationTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedPurchaseOrderPurchaseOrderStateDerivePurchaseOrder()
        {
            var order = new PurchaseOrderBuilder(this.Transaction).WithOrderedBy(this.InternalOrganisation).WithTakenViaSupplier(this.Supplier).Build();
            this.Transaction.Derive(false);

            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            var orderItem = new PurchaseOrderItemBuilder(this.Transaction)
                .WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).GseUnmotorized)
                .WithSerialisedItem(serialisedItem)
                .Build();
            order.AddPurchaseOrderItem(orderItem);
            this.Transaction.Derive(false);

            order.SetReadyForProcessing();
            this.Transaction.Derive(false);

            order.Send();
            this.Transaction.Derive(false);

            Assert.Equal(order, serialisedItem.PurchaseOrder);
        }

        [Fact]
        public void ChangedPurchaseOrderValidOrderItemsDerivePurchaseOrder()
        {
            var order = new PurchaseOrderBuilder(this.Transaction).WithOrderedBy(this.InternalOrganisation).WithTakenViaSupplier(this.Supplier).Build();
            this.Transaction.Derive(false);

            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            var orderItem = new PurchaseOrderItemBuilder(this.Transaction)
                .WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).GseUnmotorized)
                .WithSerialisedItem(serialisedItem)
                .Build();
            order.AddPurchaseOrderItem(orderItem);
            this.Transaction.Derive(false);

            order.SetReadyForProcessing();
            this.Transaction.Derive(false);

            order.Send();
            this.Transaction.Derive(false);

            Assert.Equal(order, serialisedItem.PurchaseOrder);

            orderItem.Cancel();
            this.Transaction.Derive(false, true);

            Assert.False(serialisedItem.ExistPurchaseOrder);
        }
    }

    public class SerialisedItemDerivedAssumedMonthlyOperatingHoursRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public SerialisedItemDerivedAssumedMonthlyOperatingHoursRuleTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedAssignedAssumedMonthlyOperatingHoursDeriveDerivedAssumedMonthlyOperatingHours()
        {
            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            serialisedItem.AssignedAssumedMonthlyOperatingHours = 100;
            this.Transaction.Derive(false);

            Assert.Equal(100, serialisedItem.DerivedAssumedMonthlyOperatingHours);
        }

        [Fact]
        public void ChangedPartDerivedAssumedMonthlyOperatingHoursDeriveDerivedAssumedMonthlyOperatingHours()
        {
            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            var good = new UnifiedGoodBuilder(this.Transaction).WithSerialisedItem(serialisedItem).Build();
            this.Transaction.Derive(false);

            good.AssignedAssumedMonthlyOperatingHours = 100;
            this.Transaction.Derive(false);

            Assert.Equal(100, good.DerivedAssumedMonthlyOperatingHours);
        }
    }

    public class SerialisedItemSuppliedByCountryCodeRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public SerialisedItemSuppliedByCountryCodeRuleTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedSuppliedByDeriveSuppliedByCountryCode()
        {
            var address = new PostalAddressBuilder(this.Transaction).WithCountry(new Countries(this.Transaction).CountryByIsoCode["BE"]).Build();
            var supplier = new OrganisationBuilder(this.Transaction).Build();
            new PartyContactMechanismBuilder(this.Transaction).WithParty(supplier).WithContactMechanism(address).WithContactPurpose(new ContactMechanismPurposes(this.Transaction).GeneralCorrespondence).WithUseAsDefault(true).Build();

            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            this.Derive();

            serialisedItem.AssignedSuppliedBy = supplier;
            this.Derive();

            Assert.Equal("BE", serialisedItem.SuppliedByCountryCode);
        }

        [Fact]
        public void ChangedOrganisationGeneralCorrespondenceDeriveSuppliedByCountryCode()
        {
            var supplier = new OrganisationBuilder(this.Transaction).Build();
            var serialisedItem = new SerialisedItemBuilder(this.Transaction).WithAssignedSuppliedBy(supplier).Build();
            this.Derive();

            var address = new PostalAddressBuilder(this.Transaction).WithCountry(new Countries(this.Transaction).CountryByIsoCode["BE"]).Build();
            new PartyContactMechanismBuilder(this.Transaction).WithParty(supplier).WithContactMechanism(address).WithContactPurpose(new ContactMechanismPurposes(this.Transaction).GeneralCorrespondence).WithUseAsDefault(true).Build();

            this.Derive();

            Assert.Equal("BE", serialisedItem.SuppliedByCountryCode);
        }
    }

    public class SerialisedItemSalesInvoiceNumberRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public SerialisedItemSalesInvoiceNumberRuleTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedSalesInvoiceItemSerialisedItemDeriveSalesInvoiceNumber()
        {
            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            var invoice = new SalesInvoiceBuilder(this.Transaction).WithDefaults(this.InternalOrganisation).Build();
            var invoiceItem = new SalesInvoiceItemBuilder(this.Transaction).Build();
            invoice.AddSalesInvoiceItem(invoiceItem);
            this.Derive();

            invoice.SalesInvoiceItems.First().SerialisedItem = serialisedItem;
            this.Derive();

            Assert.NotNull(invoice.InvoiceNumber);
            Assert.Equal(invoice.InvoiceNumber, serialisedItem.SalesInvoiceNumber);
        }
    }

    public class SerialisedItemSerialisedItemCharacteristicRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public SerialisedItemSerialisedItemCharacteristicRuleTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedSerialisedItemCharacteristicDeriveLength()
        {
            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var length = new SerialisedItemCharacteristicBuilder(this.Transaction).WithSerialisedItemCharacteristicType(new SerialisedItemCharacteristicTypes(this.Transaction).Length).WithValue("100").Build();
            serialisedItem.AddSerialisedItemCharacteristic(length);
            this.Transaction.Derive(false);

            Assert.Equal("100", serialisedItem.Length);
        }

        [Fact]
        public void ChangedPartSerialisedItemDeriveLength()
        {
            var lengthType = new SerialisedItemCharacteristicTypes(this.Transaction).Length;
            var productType = new ProductTypeBuilder(this.Transaction).WithSerialisedItemCharacteristicType(lengthType).Build();
            var part = new UnifiedGoodBuilder(this.Transaction).WithInventoryItemKind(new InventoryItemKinds(this.Transaction).Serialised).WithProductType(productType).Build();
            var length = new SerialisedItemCharacteristicBuilder(this.Transaction).WithSerialisedItemCharacteristicType(lengthType).WithValue("100").Build();
            part.AddSerialisedItemCharacteristic(length);
            this.Transaction.Derive(false);

            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            part.AddSerialisedItem(serialisedItem);
            this.Transaction.Derive(false);

            Assert.Equal("100", serialisedItem.Length);
        }

        [Fact]
        public void ChangedSerialisedItemCharacteristicValueDeriveLength()
        {
            var length = new SerialisedItemCharacteristicBuilder(this.Transaction).WithSerialisedItemCharacteristicType(new SerialisedItemCharacteristicTypes(this.Transaction).Length).WithValue("100").Build();
            var serialisedItem = new SerialisedItemBuilder(this.Transaction).WithSerialisedItemCharacteristic(length).Build();
            this.Transaction.Derive(false);

            length.Value = "changed";
            this.Transaction.Derive(false);

            Assert.Equal("changed", serialisedItem.Length);
        }

        [Fact]
        public void ChangedPartSerialisedItemCharacteristicValueDeriveLength()
        {
            var lengthType = new SerialisedItemCharacteristicTypes(this.Transaction).Length;
            var productType = new ProductTypeBuilder(this.Transaction).WithSerialisedItemCharacteristicType(lengthType).Build();
            var length = new SerialisedItemCharacteristicBuilder(this.Transaction).WithSerialisedItemCharacteristicType(lengthType).WithValue("100").Build();
            var part = new UnifiedGoodBuilder(this.Transaction).WithInventoryItemKind(new InventoryItemKinds(this.Transaction).Serialised).WithProductType(productType).WithSerialisedItemCharacteristic(length).Build();
            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            part.AddSerialisedItem(serialisedItem);
            this.Transaction.Derive(false);

            length.Value = "changed";
            this.Transaction.Derive(false);

            Assert.Equal("changed", serialisedItem.Length);
        }

        [Fact]
        public void ChangedSerialisedItemCharacteristicValueDeriveLengthIsNull()
        {
            var length = new SerialisedItemCharacteristicBuilder(this.Transaction).WithSerialisedItemCharacteristicType(new SerialisedItemCharacteristicTypes(this.Transaction).Length).WithValue("100").Build();
            var serialisedItem = new SerialisedItemBuilder(this.Transaction).WithSerialisedItemCharacteristic(length).Build();
            this.Transaction.Derive(false);

            Assert.NotNull(serialisedItem.Length);

            length.RemoveValue();
            this.Transaction.Derive(false);

            Assert.Null(serialisedItem.Length);
        }

        [Fact]
        public void ChangedSerialisedItemCharacteristicDeriveLengthIsNull()
        {
            var length = new SerialisedItemCharacteristicBuilder(this.Transaction).WithSerialisedItemCharacteristicType(new SerialisedItemCharacteristicTypes(this.Transaction).Length).WithValue("100").Build();
            var serialisedItem = new SerialisedItemBuilder(this.Transaction).WithSerialisedItemCharacteristic(length).Build();
            this.Transaction.Derive(false);

            Assert.NotNull(serialisedItem.Length);

            serialisedItem.RemoveSerialisedItemCharacteristic(length);
            this.Transaction.Derive(false);

            Assert.Null(serialisedItem.Length);
        }

        [Fact]
        public void ChangedProductTypeSerialisedItemCharacteristicTypeDeriveLengthNull()
        {
            var lengthType = new SerialisedItemCharacteristicTypes(this.Transaction).Length;
            var productType = new ProductTypeBuilder(this.Transaction).WithSerialisedItemCharacteristicType(lengthType).Build();
            var length = new SerialisedItemCharacteristicBuilder(this.Transaction).WithSerialisedItemCharacteristicType(lengthType).WithValue("100").Build();
            var part = new UnifiedGoodBuilder(this.Transaction).WithInventoryItemKind(new InventoryItemKinds(this.Transaction).Serialised).WithProductType(productType).WithSerialisedItemCharacteristic(length).Build();
            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            part.AddSerialisedItem(serialisedItem);
            this.Transaction.Derive(false);

            productType.RemoveSerialisedItemCharacteristicType(lengthType);
            this.Transaction.Derive(false);

            Assert.Null(serialisedItem.Length);
        }

        [Fact]
        public void ChangedPartSerialisedItemCharacteristicValueDeriveLengthNull()
        {
            var lengthType = new SerialisedItemCharacteristicTypes(this.Transaction).Length;
            var productType = new ProductTypeBuilder(this.Transaction).WithSerialisedItemCharacteristicType(lengthType).Build();
            var length = new SerialisedItemCharacteristicBuilder(this.Transaction).WithSerialisedItemCharacteristicType(lengthType).WithValue("100").Build();
            var part = new UnifiedGoodBuilder(this.Transaction).WithInventoryItemKind(new InventoryItemKinds(this.Transaction).Serialised).WithProductType(productType).WithSerialisedItemCharacteristic(length).Build();
            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            part.AddSerialisedItem(serialisedItem);
            this.Transaction.Derive(false);

            length.RemoveValue();
            this.Transaction.Derive(false);

            Assert.Null(serialisedItem.Length);
        }
    }
}
