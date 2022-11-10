using System;
using Xunit;

namespace Allors.Database.Domain.Tests
{
    public class WorkEffortPurchaseOrderItemAssignmentTests : DomainTest, IClassFixture<Fixture>
    {
        public WorkEffortPurchaseOrderItemAssignmentTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedAssignedUnitSellingPriceDeriveUnitSellingPrice()
        {
            var purchaseOrderItemAssignment = new WorkEffortPurchaseOrderItemAssignmentBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            purchaseOrderItemAssignment.AssignedUnitSellingPrice = 1;
            this.Transaction.Derive(false);

            Assert.Equal(1, purchaseOrderItemAssignment.UnitSellingPrice);
        }

        [Fact]
        public void ChangedAssignmentDeriveUnitSellingPrice()
        {
            var workTask = new WorkTaskBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var purchaseOrderItemAssignment = new WorkEffortPurchaseOrderItemAssignmentBuilder(this.Transaction)
                .WithUnitPurchasePrice(1)
                .Build();
            this.Transaction.Derive(false);

            purchaseOrderItemAssignment.Assignment = workTask;
            this.Transaction.Derive(false);

            var expected = Math.Round(1 * (1 + this.Transaction.GetSingleton().Settings.SubletSurchargePercentage / 100), 2);
            Assert.Equal(expected, purchaseOrderItemAssignment.UnitSellingPrice);
        }

        [Fact]
        public void ChangedCustomerDeriveUnitSellingPrice()
        {
            var workTask = new WorkTaskBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var purchaseOrderItemAssignment = new WorkEffortPurchaseOrderItemAssignmentBuilder(this.Transaction)
                .WithAssignment(workTask)
                .WithUnitPurchasePrice(1)
                .Build();
            this.Transaction.Derive(false);

            workTask.Customer = this.InternalOrganisation;
            this.Transaction.Derive(false);

            var expected = Math.Round(1 * (1 + this.Transaction.GetSingleton().Settings.InternalSubletSurchargePercentage / 100), 2);
            Assert.Equal(expected, purchaseOrderItemAssignment.UnitSellingPrice);
        }

        [Fact]
        public void ChangedExecutedByDeriveUnitSellingPrice()
        {
            var workTask = new WorkTaskBuilder(this.Transaction)
                .WithCustomer(this.InternalOrganisation)
                .Build();
            this.Transaction.Derive(false);

            var purchaseOrderItemAssignment = new WorkEffortPurchaseOrderItemAssignmentBuilder(this.Transaction)
                .WithAssignment(workTask)
                .WithUnitPurchasePrice(1)
                .Build();
            this.Transaction.Derive(false);

            workTask.ExecutedBy = this.InternalOrganisation;
            this.Transaction.Derive(false);

            Assert.Equal(purchaseOrderItemAssignment.UnitPurchasePrice, purchaseOrderItemAssignment.UnitSellingPrice);
        }
    }
}
