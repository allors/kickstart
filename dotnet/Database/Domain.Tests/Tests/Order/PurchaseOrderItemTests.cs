namespace Allors.Database.Domain.Tests
{
    using Allors.Database.Domain.TestPopulation;
    using System.Linq;
    using Xunit;

    public class PurchaseOrderItemTests : DomainTest, IClassFixture<Fixture>
    {
        public PurchaseOrderItemTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedInvoiceItemTypeDeriveIsReceivable()
        {
            var orderItem = new PurchaseOrderItemBuilder(this.Transaction)
                .WithPart(new NonUnifiedPartBuilder(this.Transaction).Build())
                .Build();
            this.Transaction.Derive(false);

            orderItem.InvoiceItemType = new InvoiceItemTypes(this.Transaction).GseUnmotorized;
            this.Transaction.Derive(false);

            Assert.True(orderItem.IsReceivable);
        }

        [Fact]
        public void ChangedPartDeriveIsReceivable()
        {
            var orderItem = new PurchaseOrderItemBuilder(this.Transaction)
                .WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).GseUnmotorized)
                .Build();
            this.Transaction.Derive(false);

            orderItem.Part = new NonUnifiedPartBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            Assert.True(orderItem.IsReceivable);
        }

        [Fact]
        public void ItemReceiveAddToWorkOrder()
        {
            this.InternalOrganisation.IsAutomaticallyReceived = true;

            var sparePart = this.InternalOrganisation.CreateNonSerialisedNonUnifiedPart(this.Transaction.Faker());
            var workTask = new WorkTaskBuilder(this.Transaction).WithScheduledWorkForExternalCustomer(this.InternalOrganisation).Build();
            var purchaseOrder = new PurchaseOrderBuilder(this.Transaction).WithDefaults(this.InternalOrganisation).Build();
            new SupplierOfferingBuilder(this.Transaction)
                .WithPart(sparePart)
                .WithSupplier(purchaseOrder.TakenViaSupplier)
                .WithFromDate(this.Transaction.Now().AddMinutes(-1))
                .WithUnitOfMeasure(new UnitsOfMeasure(this.Transaction).Piece)
                .WithPrice(1)
                .Build();
            this.Transaction.Derive();

            var item = new PurchaseOrderItemBuilder(this.Transaction).WithNonSerializedPartDefaults(sparePart, this.InternalOrganisation).Build();
            item.WorkTask = workTask;
            purchaseOrder.AddPurchaseOrderItem(item);
            this.Transaction.Derive();

            purchaseOrder.SetReadyForProcessing();
            this.Transaction.Derive();

            purchaseOrder.QuickReceive();
            this.Transaction.Derive();

            var inventoryAssignment = workTask.WorkEffortInventoryAssignmentsWhereAssignment.FirstOrDefault(v => v.InventoryItem.Part.Equals(sparePart));

            Assert.NotNull(inventoryAssignment);
            Assert.Equal(item.QuantityOrdered, inventoryAssignment.Quantity);
        }

        [Fact]
        public void ItemReceiveAddQuantityToWorkOrder()
        {
            this.InternalOrganisation.IsAutomaticallyReceived = true;

            var sparePart = this.InternalOrganisation.CreateNonSerialisedNonUnifiedPart(this.Transaction.Faker());
            var workTask = new WorkTaskBuilder(this.Transaction).WithScheduledWorkForExternalCustomer(this.InternalOrganisation).Build();
            var purchaseOrder = new PurchaseOrderBuilder(this.Transaction).WithDefaults(this.InternalOrganisation).Build();
            new SupplierOfferingBuilder(this.Transaction)
                .WithPart(sparePart)
                .WithSupplier(purchaseOrder.TakenViaSupplier)
                .WithFromDate(this.Transaction.Now().AddMinutes(-1))
                .WithUnitOfMeasure(new UnitsOfMeasure(this.Transaction).Piece)
                .WithPrice(1)
                .Build();
            this.Transaction.Derive();

            var inventoryItem = sparePart.InventoryItemsWherePart.FirstOrDefault(v => v.Facility.Equals(purchaseOrder.StoredInFacility));
            new InventoryItemTransactionBuilder(this.Transaction)
                .WithPart(sparePart)
                .WithReason(new InventoryTransactionReasons(this.Transaction).IncomingShipment)
                .WithFacility(purchaseOrder.StoredInFacility)
                .WithQuantity(1)
                .Build();
            this.Transaction.Derive();

            new WorkEffortInventoryAssignmentBuilder(this.Transaction).WithAssignment(workTask).WithInventoryItem(inventoryItem).WithQuantity(1).Build();
            this.Transaction.Derive();

            var item = new PurchaseOrderItemBuilder(this.Transaction).WithNonSerializedPartDefaults(sparePart, this.InternalOrganisation).Build();
            item.WorkTask = workTask;
            purchaseOrder.AddPurchaseOrderItem(item);
            this.Transaction.Derive();

            purchaseOrder.SetReadyForProcessing();
            this.Transaction.Derive();

            purchaseOrder.QuickReceive();
            this.Transaction.Derive();

            var inventoryAssignment = workTask.WorkEffortInventoryAssignmentsWhereAssignment.FirstOrDefault(v => v.InventoryItem.Part.Equals(sparePart));

            Assert.NotNull(inventoryAssignment);
            Assert.Equal(item.QuantityOrdered + 1, inventoryAssignment.Quantity);
        }
    }
}
