using System.Linq;
using Xunit;

namespace Allors.Database.Domain.Tests
{
    public class WorkTaskTests : DomainTest, IClassFixture<Fixture>
    {
        public WorkTaskTests(Fixture fixture) : base(fixture) { }

        //TODO: Martien, reactivate test
        [Fact(Skip = "temporary disabled")]
        public void OnInitTest()
        {
            var workEffort = new WorkTaskBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            Assert.Equal(2, workEffort.WorkEffortInvoiceItemAssignmentsWhereAssignment.Count());
        }
    }

    public class WorkEffortTotalCostRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public WorkEffortTotalCostRuleTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedTimeEntryWorkEffortDeriveTotalLabourCost()
        {
            var workEffort = new WorkTaskBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var timeEnty = new TimeEntryBuilder(this.Transaction)
                .WithAssignedBillingRate(10)
                .WithAssignedAmountOfTime(1)
                .Build();
            this.Transaction.Derive(false);

            timeEnty.WorkEffort = workEffort;
            this.Transaction.Derive(false);

            Assert.Equal(10, workEffort.TotalLabourCost);
        }

        [Fact]
        public void ChangedTimeEntryCostDeriveTotalLabourCost()
        {
            var workEffort = new WorkTaskBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var timeEnty = new TimeEntryBuilder(this.Transaction)
                .WithAssignedBillingRate(10)
                .WithAssignedAmountOfTime(1)
                .WithWorkEffort(workEffort)
                .Build();
            this.Transaction.Derive(false);

            timeEnty.AssignedBillingRate = 11;
            this.Transaction.Derive(false);

            Assert.Equal(11, workEffort.TotalLabourCost);
        }

        [Fact]
        public void ChangedWorkEffortInventoryAssignmentAssignmentDeriveTotalMaterialCost()
        {
            var part = new NonUnifiedPartBuilder(this.Transaction).WithInventoryItemKind(new InventoryItemKinds(this.Transaction).NonSerialised).Build();
            this.Transaction.Derive(false);

            new SupplierOfferingBuilder(this.Transaction)
                .WithSupplier(this.Supplier)
                .WithPart(part)
                .WithPrice(10)
                .Build();
            this.Transaction.Derive(false);

            var workEffort = new WorkTaskBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var workEffortInventoryAssignment = new WorkEffortInventoryAssignmentBuilder(this.Transaction)
                .WithInventoryItem(part.InventoryItemsWherePart.First())
                .WithQuantity(2)
                .Build();
            this.Transaction.Derive(false);

            workEffortInventoryAssignment.Assignment = workEffort;
            this.Transaction.Derive(false);

            Assert.Equal(20, workEffort.TotalMaterialCost);
        }

        [Fact]
        public void ChangedWorkEffortInventoryAssignmentCostOfGoodsSoldDeriveTotalMaterialCost()
        {
            var part = new NonUnifiedPartBuilder(this.Transaction).WithInventoryItemKind(new InventoryItemKinds(this.Transaction).NonSerialised).Build();
            this.Transaction.Derive(false);

            new SupplierOfferingBuilder(this.Transaction)
                .WithSupplier(this.Supplier)
                .WithPart(part)
                .WithPrice(10)
                .Build();
            this.Transaction.Derive(false);

            var workEffort = new WorkTaskBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var workEffortInventoryAssignment = new WorkEffortInventoryAssignmentBuilder(this.Transaction)
                .WithAssignment(workEffort)
                .WithInventoryItem(part.InventoryItemsWherePart.First())
                .WithQuantity(2)
                .Build();
            this.Transaction.Derive(false);

            workEffortInventoryAssignment.Quantity = 3;
            this.Transaction.Derive(false, true);

            Assert.Equal(30, workEffort.TotalMaterialCost);
        }

        [Fact]
        public void ChangedWorkEffortPurchaseOrderItemAssignmentAssignmentDeriveTotalSubContractedCost()
        {
            var workEffort = new WorkTaskBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var purchaseOrderItemAssignment = new WorkEffortPurchaseOrderItemAssignmentBuilder(this.Transaction)
                .WithQuantity(2)
                .WithUnitPurchasePrice(10)
                .WithAssignedUnitSellingPrice(20)
                .Build();
            this.Transaction.Derive(false);

            purchaseOrderItemAssignment.Assignment = workEffort;
            this.Transaction.Derive(false);

            Assert.Equal(20, workEffort.TotalSubContractedCost);
        }

        [Fact]
        public void ChangedWorkEffortPurchaseOrderItemAssignmentQuantityDeriveTotalSubContractedCost()
        {
            var workEffort = new WorkTaskBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var purchaseOrderItemAssignment = new WorkEffortPurchaseOrderItemAssignmentBuilder(this.Transaction)
                .WithAssignment(workEffort)
                .WithQuantity(2)
                .WithUnitPurchasePrice(10)
                .WithAssignedUnitSellingPrice(20)
                .Build();
            this.Transaction.Derive(false);

            purchaseOrderItemAssignment.Quantity = 1;
            this.Transaction.Derive(false);

            Assert.Equal(10, workEffort.TotalSubContractedCost);
        }

        [Fact]
        public void ChangedWorkEffortPurchaseOrderItemAssignmentUnitPurchasePriceDeriveTotalSubContractedCost()
        {
            var workEffort = new WorkTaskBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var purchaseOrderItemAssignment = new WorkEffortPurchaseOrderItemAssignmentBuilder(this.Transaction)
                .WithAssignment(workEffort)
                .WithQuantity(2)
                .WithUnitPurchasePrice(10)
                .WithAssignedUnitSellingPrice(20)
                .Build();
            this.Transaction.Derive(false);

            purchaseOrderItemAssignment.UnitPurchasePrice = 5;
            this.Transaction.Derive(false);

            Assert.Equal(10, workEffort.TotalSubContractedCost);
        }
    }

    public class WorkEffortTotalLabourRevenueRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public WorkEffortTotalLabourRevenueRuleTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedTimeEntryWorkEffortDeriveTotalLabourRevenue()
        {
            var workEffort = new WorkTaskBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var timeEntry = new TimeEntryBuilder(this.Transaction)
                .WithIsBillable(true)
                .WithBillingFrequency(new TimeFrequencies(this.Transaction).Hour)
                .WithAssignedBillingRate(10)
                .WithAssignedAmountOfTime(1)
                .Build();
            this.Transaction.Derive(false);

            timeEntry.WorkEffort = workEffort;
            this.Transaction.Derive(false);

            Assert.Equal(10, workEffort.TotalLabourRevenue);
        }

        [Fact]
        public void ChangedTimeEntryBillingAmountDeriveTotalLabourRevenue()
        {
            var workEffort = new WorkTaskBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var timeEntry = new TimeEntryBuilder(this.Transaction)
                .WithWorkEffort(workEffort)
                .WithIsBillable(true)
                .WithBillingFrequency(new TimeFrequencies(this.Transaction).Hour)
                .WithAssignedBillingRate(10)
                .WithAssignedAmountOfTime(1)
                .Build();
            this.Transaction.Derive(false);

            timeEntry.AssignedBillingRate = 11;
            this.Transaction.Derive(false);

            Assert.Equal(11, workEffort.TotalLabourRevenue);
        }

        [Fact]
        public void ChangedTimeEntryIsBillableDeriveTotalLabourRevenue()
        {
            var workEffort = new WorkTaskBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var timeEntry = new TimeEntryBuilder(this.Transaction)
                .WithWorkEffort(workEffort)
                .WithIsBillable(false)
                .WithBillingFrequency(new TimeFrequencies(this.Transaction).Hour)
                .WithAssignedBillingRate(10)
                .WithAssignedAmountOfTime(1)
                .Build();
            this.Transaction.Derive(false);

            timeEntry.IsBillable = true;
            this.Transaction.Derive(false);

            Assert.Equal(10, workEffort.TotalLabourRevenue);
        }

        [Fact]
        public void ChangedTimeEntryAmountOfTimeDeriveTotalLabourRevenue()
        {
            var workEffort = new WorkTaskBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var timeEntry = new TimeEntryBuilder(this.Transaction)
                .WithWorkEffort(workEffort)
                .WithIsBillable(true)
                .WithBillingFrequency(new TimeFrequencies(this.Transaction).Hour)
                .WithAssignedBillingRate(10)
                .WithAssignedAmountOfTime(1)
                .Build();
            this.Transaction.Derive(false);

            timeEntry.RemoveThroughDate();
            timeEntry.AssignedAmountOfTime = 2;
            this.Transaction.Derive(false);

            Assert.Equal(20, workEffort.TotalLabourRevenue);
        }

        [Fact]
        public void ChangedTimeEntryBillableAmountOfTimeDeriveTotalLabourRevenue()
        {
            var workEffort = new WorkTaskBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var timeEntry = new TimeEntryBuilder(this.Transaction)
                .WithWorkEffort(workEffort)
                .WithIsBillable(true)
                .WithBillingFrequency(new TimeFrequencies(this.Transaction).Hour)
                .WithAssignedBillingRate(10)
                .WithAssignedAmountOfTime(1)
                .Build();
            this.Transaction.Derive(false);

            timeEntry.BillableAmountOfTime = 2;
            this.Transaction.Derive(false);

            Assert.Equal(20, workEffort.TotalLabourRevenue);
        }

        [Fact]
        public void ChangedMaintenanceAgreementDeriveBillableAmountOfTimeInHours()
        {
            var workEffortType = new WorkEffortTypeBuilder(this.Transaction).WithStandardWorkHours(1).Build();
            this.Transaction.Derive(false);

            var agreeement = new MaintenanceAgreementBuilder(this.Transaction).WithWorkEffortType(workEffortType).Build();
            this.Transaction.Derive(false);
            
            var workEffort = new WorkTaskBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            new TimeEntryBuilder(this.Transaction)
                .WithWorkEffort(workEffort)
                .WithIsBillable(true)
                .WithBillingFrequency(new TimeFrequencies(this.Transaction).Hour)
                .WithAssignedBillingRate(10)
                .WithAssignedAmountOfTime(2)
                .Build();
            this.Transaction.Derive(false);

            Assert.Equal(0, workEffort.BillableAmountOfTimeInHours);

            workEffort.MaintenanceAgreement = agreeement;
            this.Transaction.Derive(false);

            Assert.Equal(1, workEffort.BillableAmountOfTimeInHours);
        }

        [Fact]
        public void ChangedTimeEntryBillableAmountOfTimeInMinutesDeriveBillableAmountOfTimeInHours()
        {
            var workEffortType = new WorkEffortTypeBuilder(this.Transaction).WithStandardWorkHours(1).Build();
            this.Transaction.Derive(false);

            var agreeement = new MaintenanceAgreementBuilder(this.Transaction).WithWorkEffortType(workEffortType).Build();
            this.Transaction.Derive(false);

            var workEffort = new WorkTaskBuilder(this.Transaction).WithMaintenanceAgreement(agreeement).Build();
            this.Transaction.Derive(false);

            var timeEntry = new TimeEntryBuilder(this.Transaction)
                .WithWorkEffort(workEffort)
                .WithIsBillable(true)
                .WithBillingFrequency(new TimeFrequencies(this.Transaction).Hour)
                .WithAssignedBillingRate(10)
                .WithAssignedAmountOfTime(2)
                .Build();
            this.Transaction.Derive(false);

            Assert.Equal(1, workEffort.BillableAmountOfTimeInHours);

            timeEntry.RemoveThroughDate();
            timeEntry.AssignedAmountOfTime = 3;
            this.Transaction.Derive(false);

            Assert.Equal(2, workEffort.BillableAmountOfTimeInHours);
        }

        [Fact]
        public void ChangedMaintenanceAgreementWorkEffortTypeDeriveBillableAmountOfTimeInHours()
        {
            var workEffortType1 = new WorkEffortTypeBuilder(this.Transaction).WithStandardWorkHours(1).Build();
            var workEffortType2 = new WorkEffortTypeBuilder(this.Transaction).WithStandardWorkHours(2).Build();
            this.Transaction.Derive(false);

            var agreeement = new MaintenanceAgreementBuilder(this.Transaction).WithWorkEffortType(workEffortType1).Build();
            this.Transaction.Derive(false);

            var workEffort = new WorkTaskBuilder(this.Transaction).WithMaintenanceAgreement(agreeement).Build();
            this.Transaction.Derive(false);

            new TimeEntryBuilder(this.Transaction)
                .WithWorkEffort(workEffort)
                .WithIsBillable(true)
                .WithBillingFrequency(new TimeFrequencies(this.Transaction).Hour)
                .WithAssignedBillingRate(10)
                .WithAssignedAmountOfTime(3)
                .Build();
            this.Transaction.Derive(false);

            Assert.Equal(2, workEffort.BillableAmountOfTimeInHours);

            agreeement.WorkEffortType = workEffortType2;
            this.Transaction.Derive(false);

            Assert.Equal(1, workEffort.BillableAmountOfTimeInHours);
        }

        [Fact]
        public void ChangedWorkEffortTypeStandardWorkHoursDeriveBillableAmountOfTimeInHours()
        {
            var workEffortType = new WorkEffortTypeBuilder(this.Transaction).WithStandardWorkHours(1).Build();
            this.Transaction.Derive(false);

            var agreeement = new MaintenanceAgreementBuilder(this.Transaction).WithWorkEffortType(workEffortType).Build();
            this.Transaction.Derive(false);

            var workEffort = new WorkTaskBuilder(this.Transaction).WithMaintenanceAgreement(agreeement).Build();
            this.Transaction.Derive(false);

            new TimeEntryBuilder(this.Transaction)
                .WithWorkEffort(workEffort)
                .WithIsBillable(true)
                .WithBillingFrequency(new TimeFrequencies(this.Transaction).Hour)
                .WithAssignedBillingRate(10)
                .WithAssignedAmountOfTime(3)
                .Build();
            this.Transaction.Derive(false);

            Assert.Equal(2, workEffort.BillableAmountOfTimeInHours);

            workEffortType.StandardWorkHours = 2;
            this.Transaction.Derive(false);

            Assert.Equal(1, workEffort.BillableAmountOfTimeInHours);
        }
    }

    public class AviationWorkTaskRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public AviationWorkTaskRuleTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedWorkEffortFixedAssetAssignmentFixedAssetDeriveCustomer()
        {
            var anotherInternalOrganisation = new OrganisationBuilder(this.Transaction).WithIsInternalOrganisation(true).Build();
            var part = new UnifiedGoodBuilder(this.Transaction).WithInventoryItemKind(new InventoryItemKinds(this.Transaction).Serialised).Build();
            var serialisedItem = new SerialisedItemBuilder(this.Transaction)
                .WithOwnedBy(anotherInternalOrganisation)
                .Build();
            part.AddSerialisedItem(serialisedItem);
            this.Transaction.Derive(false);

            var worktask = new WorkTaskBuilder(this.Transaction)
                .WithCustomer(this.InternalOrganisation)
                .Build();
            this.Transaction.Derive(false);

            var workEffortFixedAssetAssignment = new WorkEffortFixedAssetAssignmentBuilder(this.Transaction)
                .WithAssignment(worktask)
                .Build();
            this.Transaction.Derive(false);

            workEffortFixedAssetAssignment.FixedAsset = serialisedItem;
            this.Transaction.Derive(false);

            Assert.Equal(anotherInternalOrganisation, worktask.Customer);
        }

        [Fact]
        public void ChangedCustomerDeriveCustomer()
        {
            var anotherInternalOrganisation = new OrganisationBuilder(this.Transaction).WithIsInternalOrganisation(true).Build();
            var part = new UnifiedGoodBuilder(this.Transaction).WithInventoryItemKind(new InventoryItemKinds(this.Transaction).Serialised).Build();
            var serialisedItem = new SerialisedItemBuilder(this.Transaction)
                .WithOwnedBy(anotherInternalOrganisation)
                .Build();
            part.AddSerialisedItem(serialisedItem);
            this.Transaction.Derive(false);

            var worktask = new WorkTaskBuilder(this.Transaction)
                .WithCustomer(this.Customer)
                .Build();
            this.Transaction.Derive(false);

            new WorkEffortFixedAssetAssignmentBuilder(this.Transaction)
                .WithAssignment(worktask)
                .WithFixedAsset(serialisedItem)
                .Build();
            this.Transaction.Derive(false);

            worktask.Customer = this.InternalOrganisation;
            this.Transaction.Derive(false);

            Assert.Equal(anotherInternalOrganisation, worktask.Customer);
        }

        [Fact]
        public void ChangedWorkEffortStateDeriveCustomer()
        {
            var anotherInternalOrganisation = new OrganisationBuilder(this.Transaction).WithIsInternalOrganisation(true).Build();
            var part = new UnifiedGoodBuilder(this.Transaction).WithInventoryItemKind(new InventoryItemKinds(this.Transaction).Serialised).Build();
            var serialisedItem = new SerialisedItemBuilder(this.Transaction)
                .WithOwnedBy(anotherInternalOrganisation)
                .Build();
            part.AddSerialisedItem(serialisedItem);
            this.Transaction.Derive(false);

            var worktask = new WorkTaskBuilder(this.Transaction)
                .WithCustomer(this.InternalOrganisation)
                .WithWorkEffortState(new WorkEffortStates(this.Transaction).Cancelled)
                .Build();
            this.Transaction.Derive(false);

            new WorkEffortFixedAssetAssignmentBuilder(this.Transaction)
                .WithAssignment(worktask)
                .WithFixedAsset(serialisedItem)
                .Build();
            this.Transaction.Derive(false);

            worktask.WorkEffortState = new WorkEffortStates(this.Transaction).Created;
            this.Transaction.Derive(false);

            Assert.Equal(anotherInternalOrganisation, worktask.Customer);
        }

        [Fact]
        public void ChangedSerialisedItemOwnedByDeriveCustomer()
        {
            var anotherInternalOrganisation = new OrganisationBuilder(this.Transaction).WithIsInternalOrganisation(true).Build();
            var part = new UnifiedGoodBuilder(this.Transaction).WithInventoryItemKind(new InventoryItemKinds(this.Transaction).Serialised).Build();
            var serialisedItem = new SerialisedItemBuilder(this.Transaction)
                .WithOwnedBy(this.Customer)
                .Build();
            part.AddSerialisedItem(serialisedItem);
            this.Transaction.Derive(false);

            var worktask = new WorkTaskBuilder(this.Transaction)
                .WithCustomer(this.InternalOrganisation)
                .Build();
            this.Transaction.Derive(false);

            new WorkEffortFixedAssetAssignmentBuilder(this.Transaction)
                .WithAssignment(worktask)
                .WithFixedAsset(serialisedItem)
                .Build();
            this.Transaction.Derive(false);

            serialisedItem.OwnedBy = anotherInternalOrganisation;
            this.Transaction.Derive(false);

            Assert.Equal(anotherInternalOrganisation, worktask.Customer);
        }
    }
}
