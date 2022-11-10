using System;
using System.Linq;
using Allors.Database.Domain.TestPopulation;

using Xunit;

namespace Allors.Database.Domain.Tests
{
    public class WorkEffortInventoryAssignmentTests : DomainTest, IClassFixture<Fixture>
    {
        public WorkEffortInventoryAssignmentTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void GivenWorkTaskWithoutMaintenanceAgreement_WhenPartIsAssignedWithAssignedUnitPrice_ThenUnitSellingPriceIsEqualAssignedUnitPrice()
        {
            var sparePart = this.InternalOrganisation.CreateNonSerialisedNonUnifiedPart(this.Transaction.Faker());
            var workTask = new WorkTaskBuilder(this.Transaction).WithScheduledWorkForExternalCustomer(this.InternalOrganisation).Build();

            this.Transaction.Derive();

            var inventoryAssignment = new WorkEffortInventoryAssignmentBuilder(this.Transaction)
                .WithAssignment(workTask)
                .WithInventoryItem(sparePart.InventoryItemsWherePart.First())
                .WithQuantity(2)
                .WithAssignedUnitSellingPrice(10M)
                .Build();

            this.Transaction.Derive();

            Assert.Equal(inventoryAssignment.AssignedUnitSellingPrice, inventoryAssignment.UnitSellingPrice);
        }

        [Fact]
        public void GivenWorkTaskWithoutMaintenanceAgreement_WhenPartIsAssigned_ThenUnitSellingPriceIsIsCalculatedUsingPartSurchargePercentage()
        {
            var sparePart = this.InternalOrganisation.CreateNonSerialisedNonUnifiedPart(this.Transaction.Faker());
            var workTask = new WorkTaskBuilder(this.Transaction).WithScheduledWorkForExternalCustomer(this.InternalOrganisation).Build();

            this.Transaction.Derive();

            var inventoryAssignment = new WorkEffortInventoryAssignmentBuilder(this.Transaction)
                .WithAssignment(workTask)
                .WithInventoryItem(sparePart.InventoryItemsWherePart.First())
                .WithQuantity(2)
                .Build();

            this.Transaction.Derive();

            var partSurchargePercentage = this.Transaction.GetSingleton().Settings.PartSurchargePercentage;
            var purchasePrice = sparePart.SupplierOfferingsWherePart.First().Price;
            var expectedSellingPrice = Math.Round(purchasePrice * (1 + partSurchargePercentage / 100), 2);

            Assert.Equal(expectedSellingPrice, inventoryAssignment.UnitSellingPrice);
        }

        [Fact]
        public void GivenWorkTaskForInternalWork_WhenPartIsAssigned_ThenUnitSellingPriceIsCalculatedUsingInternalPartSurchargePercentage()
        {
            new OrganisationBuilder(this.Transaction)
                .WithIsInternalOrganisation(true)
                .WithName("internalOrganisation")
                .WithPreferredCurrency(new Currencies(this.Transaction).CurrencyByCode["EUR"])
                .WithPurchaseShipmentNumberPrefix("incoming shipmentno: ")
                .WithPurchaseInvoiceNumberPrefix("incoming invoiceno: ")
                .WithPurchaseOrderNumberPrefix("purchase orderno: ")
                .Build();

            var sparePart = this.InternalOrganisation.CreateNonSerialisedNonUnifiedPart(this.Transaction.Faker());
            var workTask = new WorkTaskBuilder(this.Transaction).WithScheduledInternalWork(this.InternalOrganisation).Build();

            this.Transaction.Derive();

            var inventoryAssignment = new WorkEffortInventoryAssignmentBuilder(this.Transaction)
                .WithAssignment(workTask)
                .WithInventoryItem(sparePart.InventoryItemsWherePart.First())
                .WithQuantity(2)
                .Build();

            this.Transaction.Derive();

            var internalPartSurchargePercentage = this.Transaction.GetSingleton().Settings.InternalPartSurchargePercentage;
            var purchasePrice = sparePart.SupplierOfferingsWherePart.First().Price;
            var expectedSellingPrice = Math.Round(purchasePrice * (1 + internalPartSurchargePercentage / 100), 2);

            Assert.Equal(expectedSellingPrice, inventoryAssignment.UnitSellingPrice);
        }

        [Fact]
        public void GivenWorkTaskUnderAgreement_WhenExtraPartIsAssigned_ThenUnitSellingPriceIsCalculatedUsingAgreementPartSurchargePercentage()
        {
            var sparePart = this.InternalOrganisation.CreateNonSerialisedNonUnifiedPart(this.Transaction.Faker());
            var unifiedGood = new UnifiedGoodBuilder(this.Transaction).WithSerialisedDefaults(this.InternalOrganisation).Build();
            var workTask = new WorkTaskBuilder(this.Transaction).WithScheduledWorkForExternalCustomer(this.InternalOrganisation).Build();

            this.Transaction.Derive();

            var workEffortType = new WorkEffortTypeBuilder(this.Transaction)
                .WithName("name")
                .WithDescription("description")
                .WithUnifiedGood(unifiedGood)
                .Build();

            var maintenanceAgreement = new MaintenanceAgreementBuilder(this.Transaction)
                .WithWorkEffortType(workEffortType)
                .WithDescription("Description")
                .WithPartSurchargePercentage(20)
                .Build();

            workTask.Customer.CustomerRelationshipsWhereCustomer.First().AddAgreement(maintenanceAgreement);
            workTask.MaintenanceAgreement = maintenanceAgreement;

            this.Transaction.Derive();

            var inventoryAssignment = new WorkEffortInventoryAssignmentBuilder(this.Transaction)
                .WithAssignment(workTask)
                .WithInventoryItem(sparePart.InventoryItemsWherePart.First())
                .WithQuantity(2)
                .Build();

            this.Transaction.Derive();

            var purchasePrice = sparePart.SupplierOfferingsWherePart.First().Price;
            var expectedSellingPrice = Math.Round(purchasePrice * (1 + maintenanceAgreement.PartSurchargePercentage / 100), 2);

            Assert.Equal(expectedSellingPrice, inventoryAssignment.UnitSellingPrice);
            Assert.Equal(0, inventoryAssignment.WorkEffortStandardQuantity);
            Assert.Equal(inventoryAssignment.Quantity, inventoryAssignment.ExtraQuantity);
        }

        [Fact]
        public void GivenWorkTaskUnderAgreement_WhenPartQuantityIsWithinAgreement_ThenUnitSellingPriceIsZero()
        {
            var sparePart = this.InternalOrganisation.CreateNonSerialisedNonUnifiedPart(this.Transaction.Faker());
            var unifiedGood = new UnifiedGoodBuilder(this.Transaction).WithSerialisedDefaults(this.InternalOrganisation).Build();
            var workTask = new WorkTaskBuilder(this.Transaction).WithScheduledWorkForExternalCustomer(this.InternalOrganisation).Build();

            this.Transaction.Derive();

            var workEffortType = new WorkEffortTypeBuilder(this.Transaction)
                .WithName("name")
                .WithDescription("description")
                .WithUnifiedGood(unifiedGood)
                .Build();

            var maintenanceAgreement = new MaintenanceAgreementBuilder(this.Transaction)
                .WithWorkEffortType(workEffortType)
                .WithDescription("Description")
                .WithPartSurchargePercentage(20)
                .Build();

            workTask.Customer.CustomerRelationshipsWhereCustomer.First().AddAgreement(maintenanceAgreement);
            workTask.MaintenanceAgreement = maintenanceAgreement;

            workEffortType.AddWorkEffortPartStandard(new WorkEffortPartStandardBuilder(this.Transaction).WithPart(sparePart).WithQuantity(1).Build());

            this.Transaction.Derive();

            var inventoryAssignment1 = new WorkEffortInventoryAssignmentBuilder(this.Transaction)
                .WithAssignment(workTask)
                .WithInventoryItem(sparePart.InventoryItemsWherePart.First())
                .WithQuantity(1)
                .Build();

            this.Transaction.Derive();

            Assert.Equal(0, inventoryAssignment1.UnitSellingPrice);
            Assert.Equal(1, inventoryAssignment1.WorkEffortStandardQuantity);
            Assert.Equal(0, inventoryAssignment1.ExtraQuantity);

            var inventoryAssignment2 = new WorkEffortInventoryAssignmentBuilder(this.Transaction)
                .WithAssignment(workTask)
                .WithInventoryItem(sparePart.InventoryItemsWherePart.First())
                .WithQuantity(2)
                .WithAssignedBillableQuantity(1)
                .Build();

            this.Transaction.Derive();

            Assert.Equal(0, inventoryAssignment2.UnitSellingPrice);
            Assert.Equal(1, inventoryAssignment1.WorkEffortStandardQuantity);
            Assert.Equal(0, inventoryAssignment1.ExtraQuantity);
        }

        [Fact]
        public void GivenWorkTaskUnderAgreement_WhenPartQuantityIsOutsideAgreement_ThenExtraQuantityIsCalculatedUsingAgreementPartSurchargePercentage()
        {
            var sparePart = this.InternalOrganisation.CreateNonSerialisedNonUnifiedPart(this.Transaction.Faker());
            var unifiedGood = new UnifiedGoodBuilder(this.Transaction).WithSerialisedDefaults(this.InternalOrganisation).Build();
            var workTask = new WorkTaskBuilder(this.Transaction).WithScheduledWorkForExternalCustomer(this.InternalOrganisation).Build();

            this.Transaction.Derive();

            var workEffortType = new WorkEffortTypeBuilder(this.Transaction)
                .WithName("name")
                .WithDescription("description")
                .WithUnifiedGood(unifiedGood)
                .Build();

            var maintenanceAgreement = new MaintenanceAgreementBuilder(this.Transaction)
                .WithWorkEffortType(workEffortType)
                .WithDescription("Description")
                .WithPartSurchargePercentage(20)
                .Build();

            workTask.Customer.CustomerRelationshipsWhereCustomer.First().AddAgreement(maintenanceAgreement);
            workTask.MaintenanceAgreement = maintenanceAgreement;

            workEffortType.AddWorkEffortPartStandard(new WorkEffortPartStandardBuilder(this.Transaction).WithPart(sparePart).WithQuantity(1).Build());

            this.Transaction.Derive();

            var inventoryAssignment = new WorkEffortInventoryAssignmentBuilder(this.Transaction)
                .WithAssignment(workTask)
                .WithInventoryItem(sparePart.InventoryItemsWherePart.First())
                .WithQuantity(2)
                .Build();

            this.Transaction.Derive();

            var purchasePrice = sparePart.SupplierOfferingsWherePart.First().Price;
            var expectedSellingPrice = Math.Round(purchasePrice * (1 + maintenanceAgreement.PartSurchargePercentage / 100), 2);

            Assert.Equal(expectedSellingPrice, inventoryAssignment.UnitSellingPrice);
            Assert.Equal(1, inventoryAssignment.WorkEffortStandardQuantity);
            Assert.Equal(1, inventoryAssignment.ExtraQuantity);
            Assert.Equal(1, inventoryAssignment.DerivedBillableQuantity);
        }
    }

    public class WorkEffortInventoryAssignmentCostOfGoodsSoldRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public WorkEffortInventoryAssignmentCostOfGoodsSoldRuleTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedInventoryItemDeriveCostOfGoodsSold()
        {
            var part = new NonUnifiedPartBuilder(this.Transaction)
                .WithInventoryItemKind(new InventoryItemKinds(this.Transaction).NonSerialised)
                .Build();
            this.Transaction.Derive(false);

            new InventoryItemTransactionBuilder(this.Transaction)
                .WithPart(part)
                .WithReason(new InventoryTransactionReasons(this.Transaction).IncomingShipment)
                .WithQuantity(3)
                .WithCost(10)
                .Build();
            this.Transaction.Derive(false);

            new SupplierOfferingBuilder(this.Transaction)
                .WithPart(part)
                .WithSupplier(this.Supplier)
                .WithPrice(20)
                .Build();
            this.Transaction.Derive(false);

            var workEffort = new WorkTaskBuilder(this.Transaction).Build();
            var inventoryAssignment = new WorkEffortInventoryAssignmentBuilder(this.Transaction)
                .WithAssignment(workEffort)
                .WithInventoryItem(part.InventoryItemsWherePart.First())
                .WithQuantity(2)
                .Build();
            this.Transaction.Derive(false);

            Assert.Equal(40, inventoryAssignment.CostOfGoodsSold);
        }

        [Fact]
        public void ChangedQuantityDeriveCostOfGoodsSold()
        {
            var part = new NonUnifiedPartBuilder(this.Transaction)
                .WithInventoryItemKind(new InventoryItemKinds(this.Transaction).NonSerialised)
                .Build();
            this.Transaction.Derive(false);

            new SupplierOfferingBuilder(this.Transaction)
                .WithPart(part)
                .WithSupplier(this.Supplier)
                .WithPrice(20)
                .Build();
            this.Transaction.Derive(false);

            var workEffort = new WorkTaskBuilder(this.Transaction).Build();
            new InventoryItemTransactionBuilder(this.Transaction)
                .WithPart(part)
                .WithReason(new InventoryTransactionReasons(this.Transaction).IncomingShipment)
                .WithQuantity(3)
                .WithCost(10)
                .Build();
            this.Transaction.Derive(false);

            var inventoryAssignment = new WorkEffortInventoryAssignmentBuilder(this.Transaction)
                .WithAssignment(workEffort)
                .WithInventoryItem(part.InventoryItemsWherePart.First())
                .WithQuantity(2)
                .Build();
            this.Transaction.Derive(false);

            inventoryAssignment.Quantity = 3;
            this.Transaction.Derive(false);

            Assert.Equal(60, inventoryAssignment.CostOfGoodsSold);
        }

        [Fact]
        public void ChangedWorkEffortWorkEffortStateDeriveCostOfGoodsSold()
        {
            var part = new NonUnifiedPartBuilder(this.Transaction)
                .WithInventoryItemKind(new InventoryItemKinds(this.Transaction).NonSerialised)
                .Build();
            this.Transaction.Derive(false);

            new SupplierOfferingBuilder(this.Transaction)
                .WithPart(part)
                .WithSupplier(this.Supplier)
                .WithPrice(20)
                .Build();
            this.Transaction.Derive(false);

            var workEffort = new WorkTaskBuilder(this.Transaction).WithWorkEffortState(new WorkEffortStates(this.Transaction).Completed).Build();
            new InventoryItemTransactionBuilder(this.Transaction)
                .WithPart(part)
                .WithReason(new InventoryTransactionReasons(this.Transaction).IncomingShipment)
                .WithQuantity(3)
                .WithCost(10)
                .Build();
            this.Transaction.Derive(false);

            var inventoryAssignment = new WorkEffortInventoryAssignmentBuilder(this.Transaction)
                .WithAssignment(workEffort)
                .WithInventoryItem(part.InventoryItemsWherePart.First())
                .WithQuantity(2)
                .Build();
            this.Transaction.Derive(false);

            Assert.Equal(0, inventoryAssignment.CostOfGoodsSold);

            workEffort.WorkEffortState = new WorkEffortStates(this.Transaction).Created;
            this.Transaction.Derive(false);

            Assert.Equal(40, inventoryAssignment.CostOfGoodsSold);
        }

        [Fact]
        public void ChangedSupplierOfferingPartDeriveCostOfGoodsSold()
        {
            var part = new NonUnifiedPartBuilder(this.Transaction)
                .WithInventoryItemKind(new InventoryItemKinds(this.Transaction).NonSerialised)
                .Build();
            this.Transaction.Derive(false);

            var workEffort = new WorkTaskBuilder(this.Transaction).Build();
            new InventoryItemTransactionBuilder(this.Transaction)
                .WithPart(part)
                .WithReason(new InventoryTransactionReasons(this.Transaction).IncomingShipment)
                .WithQuantity(3)
                .WithCost(10)
                .Build();
            this.Transaction.Derive(false);

            var inventoryAssignment = new WorkEffortInventoryAssignmentBuilder(this.Transaction)
                .WithAssignment(workEffort)
                .WithInventoryItem(part.InventoryItemsWherePart.First())
                .WithQuantity(2)
                .Build();
            this.Transaction.Derive(false);

            new SupplierOfferingBuilder(this.Transaction)
                .WithPart(part)
                .WithSupplier(this.Supplier)
                .WithPrice(20)
                .Build();
            this.Transaction.Derive(false);

            Assert.Equal(40, inventoryAssignment.CostOfGoodsSold);
        }

        [Fact]
        public void ChangedSupplierOfferingPriceDeriveCostOfGoodsSold()
        {
            var part = new NonUnifiedPartBuilder(this.Transaction)
                .WithInventoryItemKind(new InventoryItemKinds(this.Transaction).NonSerialised)
                .Build();
            this.Transaction.Derive(false);

            var workEffort = new WorkTaskBuilder(this.Transaction).Build();
            new InventoryItemTransactionBuilder(this.Transaction)
                .WithPart(part)
                .WithReason(new InventoryTransactionReasons(this.Transaction).IncomingShipment)
                .WithQuantity(3)
                .WithCost(10)
                .Build();
            this.Transaction.Derive(false);

            var inventoryAssignment = new WorkEffortInventoryAssignmentBuilder(this.Transaction)
                .WithAssignment(workEffort)
                .WithInventoryItem(part.InventoryItemsWherePart.First())
                .WithQuantity(2)
                .Build();
            this.Transaction.Derive(false);

            var supplierOffering = new SupplierOfferingBuilder(this.Transaction)
                .WithPart(part)
                .WithSupplier(this.Supplier)
                .WithPrice(20)
                .Build();
            this.Transaction.Derive(false);

            supplierOffering.Price = 15;
            this.Transaction.Derive(false);

            Assert.Equal(30, inventoryAssignment.CostOfGoodsSold);
        }

        [Fact]
        public void ChangedInventoryItemDeriveUnitPurchasePrice()
        {
            var part = new NonUnifiedPartBuilder(this.Transaction)
                .WithInventoryItemKind(new InventoryItemKinds(this.Transaction).NonSerialised)
                .Build();
            this.Transaction.Derive(false);

            new InventoryItemTransactionBuilder(this.Transaction)
                .WithPart(part)
                .WithReason(new InventoryTransactionReasons(this.Transaction).IncomingShipment)
                .WithQuantity(3)
                .WithCost(10)
                .Build();
            this.Transaction.Derive(false);

            new SupplierOfferingBuilder(this.Transaction)
                .WithPart(part)
                .WithSupplier(this.Supplier)
                .WithPrice(20)
                .Build();
            this.Transaction.Derive(false);

            var workEffort = new WorkTaskBuilder(this.Transaction).Build();
            var inventoryAssignment = new WorkEffortInventoryAssignmentBuilder(this.Transaction)
                .WithAssignment(workEffort)
                .WithInventoryItem(part.InventoryItemsWherePart.First())
                .WithQuantity(2)
                .Build();
            this.Transaction.Derive(false);

            Assert.Equal(20, inventoryAssignment.UnitPurchasePrice);
        }

        [Fact]
        public void ChangedSupplierOfferingPartDeriveUnitPurchasePrice()
        {
            var part = new NonUnifiedPartBuilder(this.Transaction)
                .WithInventoryItemKind(new InventoryItemKinds(this.Transaction).NonSerialised)
                .Build();
            this.Transaction.Derive(false);

            var workEffort = new WorkTaskBuilder(this.Transaction).Build();
            new InventoryItemTransactionBuilder(this.Transaction)
                .WithPart(part)
                .WithReason(new InventoryTransactionReasons(this.Transaction).IncomingShipment)
                .WithQuantity(3)
                .WithCost(10)
                .Build();
            this.Transaction.Derive(false);

            var inventoryAssignment = new WorkEffortInventoryAssignmentBuilder(this.Transaction)
                .WithAssignment(workEffort)
                .WithInventoryItem(part.InventoryItemsWherePart.First())
                .WithQuantity(2)
                .Build();
            this.Transaction.Derive(false);

            new SupplierOfferingBuilder(this.Transaction)
                .WithPart(part)
                .WithSupplier(this.Supplier)
                .WithPrice(20)
                .Build();
            this.Transaction.Derive(false);

            Assert.Equal(20, inventoryAssignment.UnitPurchasePrice);
        }

        [Fact]
        public void ChangedSupplierOfferingPriceDeriveUnitPurchasePrice()
        {
            var part = new NonUnifiedPartBuilder(this.Transaction)
                .WithInventoryItemKind(new InventoryItemKinds(this.Transaction).NonSerialised)
                .Build();
            this.Transaction.Derive(false);

            new InventoryItemTransactionBuilder(this.Transaction)
                .WithPart(part)
                .WithReason(new InventoryTransactionReasons(this.Transaction).IncomingShipment)
                .WithQuantity(3)
                .WithCost(10)
                .Build();
            this.Transaction.Derive(false);

            var workEffort = new WorkTaskBuilder(this.Transaction).Build();
            var inventoryAssignment = new WorkEffortInventoryAssignmentBuilder(this.Transaction)
                .WithAssignment(workEffort)
                .WithInventoryItem(part.InventoryItemsWherePart.First())
                .WithQuantity(2)
                .Build();
            this.Transaction.Derive(false);

            var supplierOffering = new SupplierOfferingBuilder(this.Transaction)
                .WithPart(part)
                .WithSupplier(this.Supplier)
                .WithPrice(20)
                .Build();
            this.Transaction.Derive(false);

            supplierOffering.Price = 15;
            this.Transaction.Derive(false);

            Assert.Equal(15, inventoryAssignment.UnitPurchasePrice);
        }
    }

    public class WorkEffortInventoryAssignmentDerivedBillableQuantityRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public WorkEffortInventoryAssignmentDerivedBillableQuantityRuleTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedAssignedBillableQuantityDeriveDerivedBillableQuantity()
        {
            var part = new NonUnifiedPartBuilder(this.Transaction).WithInventoryItemKind(new InventoryItemKinds(this.Transaction).NonSerialised).Build();
            this.Transaction.Derive(false);

            var workEffort = new WorkTaskBuilder(this.Transaction).Build();
            var inventoryAssignment = new WorkEffortInventoryAssignmentBuilder(this.Transaction)
                .WithInventoryItem(part.InventoryItemsWherePart.First())
                .WithAssignment(workEffort)
                .WithQuantity(2)
                .Build();
            this.Transaction.Derive(false);

            inventoryAssignment.AssignedBillableQuantity = 1;
            this.Transaction.Derive(false);

            Assert.Equal(1, inventoryAssignment.DerivedBillableQuantity);
        }

        [Fact]
        public void ChangedQuantityDeriveDerivedBillableQuantity()
        {
            var part = new NonUnifiedPartBuilder(this.Transaction).WithInventoryItemKind(new InventoryItemKinds(this.Transaction).NonSerialised).Build();
            this.Transaction.Derive(false);

            var workEffort = new WorkTaskBuilder(this.Transaction).Build();
            var inventoryAssignment = new WorkEffortInventoryAssignmentBuilder(this.Transaction)
                .WithInventoryItem(part.InventoryItemsWherePart.First())
                .WithAssignment(workEffort)
                .WithQuantity(2)
                .Build();
            this.Transaction.Derive(false);

            inventoryAssignment.Quantity = 1;
            this.Transaction.Derive(false);

            Assert.Equal(1, inventoryAssignment.DerivedBillableQuantity);
        }

        [Fact]
        public void ChangedAssignmentDeriveDerivedBillableQuantity()
        {
            var part = new NonUnifiedPartBuilder(this.Transaction).WithInventoryItemKind(new InventoryItemKinds(this.Transaction).NonSerialised).Build();
            this.Transaction.Derive(false);

            var workEffort = new WorkTaskBuilder(this.Transaction).Build();
            var inventoryAssignment = new WorkEffortInventoryAssignmentBuilder(this.Transaction)
                .WithInventoryItem(part.InventoryItemsWherePart.First())
                .WithQuantity(1)
                .Build();
            this.Transaction.Derive(false);

            inventoryAssignment.Assignment = workEffort;
            this.Transaction.Derive(false);

            Assert.Equal(1, inventoryAssignment.DerivedBillableQuantity);
        }

        [Fact]
        public void ChangedWorkEffortWorkEffortStateDeriveDerivedBillableQuantity()
        {
            var part = new NonUnifiedPartBuilder(this.Transaction).WithInventoryItemKind(new InventoryItemKinds(this.Transaction).NonSerialised).Build();
            this.Transaction.Derive(false);

            var workEffort = new WorkTaskBuilder(this.Transaction).WithWorkEffortState(new WorkEffortStates(this.Transaction).Completed).Build();
            var inventoryAssignment = new WorkEffortInventoryAssignmentBuilder(this.Transaction)
                .WithAssignment(workEffort)
                .WithInventoryItem(part.InventoryItemsWherePart.First())
                .WithQuantity(1)
                .Build();
            this.Transaction.Derive(false);

            Assert.Equal(0, inventoryAssignment.DerivedBillableQuantity);

            workEffort.WorkEffortState = new WorkEffortStates(this.Transaction).Created;
            this.Transaction.Derive(false);

            Assert.Equal(1, inventoryAssignment.DerivedBillableQuantity);
        }

        [Fact]
        public void ChangedWorkTaskMaintenanceAgreementDeriveDerivedBillableQuantity()
        {
            var part = new NonUnifiedPartBuilder(this.Transaction).WithInventoryItemKind(new InventoryItemKinds(this.Transaction).NonSerialised).Build();
            this.Transaction.Derive(false);

            var workEffort = new WorkTaskBuilder(this.Transaction).Build();
            var inventoryAssignment = new WorkEffortInventoryAssignmentBuilder(this.Transaction)
                .WithInventoryItem(part.InventoryItemsWherePart.First())
                .WithAssignment(workEffort)
                .WithQuantity(1)
                .Build();
            this.Transaction.Derive(false);

            var agreement = new MaintenanceAgreementBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            workEffort.MaintenanceAgreement = agreement;
            this.Transaction.Derive(false);

            Assert.Equal(1, inventoryAssignment.DerivedBillableQuantity);
        }

        [Fact]
        public void ChangedMaintenanceAgreementWorkEffortTypeDeriveDerivedBillableQuantity()
        {
            var part = new NonUnifiedPartBuilder(this.Transaction).WithInventoryItemKind(new InventoryItemKinds(this.Transaction).NonSerialised).Build();
            this.Transaction.Derive(false);

            var partStandard = new WorkEffortPartStandardBuilder(this.Transaction).WithPart(part).WithQuantity(2).WithFromDate(this.Transaction.Now().AddDays(-1)).Build();
            this.Transaction.Derive(false);

            var workEffortType = new WorkEffortTypeBuilder(this.Transaction).WithWorkEffortPartStandard(partStandard).Build();
            this.Transaction.Derive(false);

            var agreement = new MaintenanceAgreementBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var workEffort = new WorkTaskBuilder(this.Transaction).WithMaintenanceAgreement(agreement).WithScheduledStart(this.Transaction.Now()).Build();
            this.Transaction.Derive(false);

            var inventoryAssignment = new WorkEffortInventoryAssignmentBuilder(this.Transaction)
                .WithInventoryItem(part.InventoryItemsWherePart.First())
                .WithAssignment(workEffort)
                .WithQuantity(5)
                .Build();
            this.Transaction.Derive(false);

            agreement.WorkEffortType = workEffortType;
            this.Transaction.Derive(false);

            Assert.Equal(3, inventoryAssignment.DerivedBillableQuantity);
        }

        [Fact]
        public void ChangedWorkEffortTypeWorkEffortPartStandardDeriveDerivedBillableQuantity()
        {
            var part = new NonUnifiedPartBuilder(this.Transaction).WithInventoryItemKind(new InventoryItemKinds(this.Transaction).NonSerialised).Build();
            this.Transaction.Derive(false);

            var partStandard = new WorkEffortPartStandardBuilder(this.Transaction).WithPart(part).WithQuantity(2).WithFromDate(this.Transaction.Now().AddDays(-1)).Build();
            this.Transaction.Derive(false);

            var workEffortType = new WorkEffortTypeBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var agreement = new MaintenanceAgreementBuilder(this.Transaction).WithWorkEffortType(workEffortType).Build();
            this.Transaction.Derive(false);

            var workEffort = new WorkTaskBuilder(this.Transaction).WithMaintenanceAgreement(agreement).WithScheduledStart(this.Transaction.Now()).Build();
            this.Transaction.Derive(false);

            var inventoryAssignment = new WorkEffortInventoryAssignmentBuilder(this.Transaction)
                .WithInventoryItem(part.InventoryItemsWherePart.First())
                .WithAssignment(workEffort)
                .WithQuantity(5)
                .Build();
            this.Transaction.Derive(false);

            workEffortType.AddWorkEffortPartStandard(partStandard);
            this.Transaction.Derive(false);

            Assert.Equal(3, inventoryAssignment.DerivedBillableQuantity);
        }

        [Fact]
        public void ChangedWorkEffortPartStandardFromDateDeriveDerivedBillableQuantity()
        {
            var part = new NonUnifiedPartBuilder(this.Transaction).WithInventoryItemKind(new InventoryItemKinds(this.Transaction).NonSerialised).Build();
            this.Transaction.Derive(false);

            var partStandard = new WorkEffortPartStandardBuilder(this.Transaction).WithPart(part).WithQuantity(2).WithFromDate(this.Transaction.Now().AddDays(1)).Build();
            this.Transaction.Derive(false);

            var workEffortType = new WorkEffortTypeBuilder(this.Transaction).WithWorkEffortPartStandard(partStandard).Build();
            this.Transaction.Derive(false);

            var agreement = new MaintenanceAgreementBuilder(this.Transaction).WithWorkEffortType(workEffortType).Build();
            this.Transaction.Derive(false);

            var workEffort = new WorkTaskBuilder(this.Transaction).WithMaintenanceAgreement(agreement).WithScheduledStart(this.Transaction.Now()).Build();
            this.Transaction.Derive(false);

            var inventoryAssignment = new WorkEffortInventoryAssignmentBuilder(this.Transaction)
                .WithInventoryItem(part.InventoryItemsWherePart.First())
                .WithAssignment(workEffort)
                .WithQuantity(5)
                .Build();
            this.Transaction.Derive(false);

            Assert.Equal(5, inventoryAssignment.DerivedBillableQuantity);

            partStandard.FromDate = this.Transaction.Now().AddDays(-1);
            this.Transaction.Derive(false);

            Assert.Equal(3, inventoryAssignment.DerivedBillableQuantity);
        }

        [Fact]
        public void ChangedWorkEffortPartStandardThroughDateDeriveDerivedBillableQuantity()
        {
            var part = new NonUnifiedPartBuilder(this.Transaction).WithInventoryItemKind(new InventoryItemKinds(this.Transaction).NonSerialised).Build();
            this.Transaction.Derive(false);

            var partStandard = new WorkEffortPartStandardBuilder(this.Transaction).WithPart(part).WithQuantity(2).WithFromDate(this.Transaction.Now().AddDays(-1)).WithThroughDate(this.Transaction.Now().AddDays(-1)).Build();
            this.Transaction.Derive(false);

            var workEffortType = new WorkEffortTypeBuilder(this.Transaction).WithWorkEffortPartStandard(partStandard).Build();
            this.Transaction.Derive(false);

            var agreement = new MaintenanceAgreementBuilder(this.Transaction).WithWorkEffortType(workEffortType).Build();
            this.Transaction.Derive(false);

            var workEffort = new WorkTaskBuilder(this.Transaction).WithMaintenanceAgreement(agreement).WithScheduledStart(this.Transaction.Now()).Build();
            this.Transaction.Derive(false);

            var inventoryAssignment = new WorkEffortInventoryAssignmentBuilder(this.Transaction)
                .WithInventoryItem(part.InventoryItemsWherePart.First())
                .WithAssignment(workEffort)
                .WithQuantity(5)
                .Build();
            this.Transaction.Derive(false);

            Assert.Equal(5, inventoryAssignment.DerivedBillableQuantity);

            partStandard.RemoveThroughDate();
            this.Transaction.Derive(false);

            Assert.Equal(3, inventoryAssignment.DerivedBillableQuantity);
        }

        [Fact]
        public void ChangedWorkEffortPartStandardQuantityDeriveDerivedBillableQuantity()
        {
            var part = new NonUnifiedPartBuilder(this.Transaction).WithInventoryItemKind(new InventoryItemKinds(this.Transaction).NonSerialised).Build();
            this.Transaction.Derive(false);

            var partStandard = new WorkEffortPartStandardBuilder(this.Transaction).WithPart(part).WithQuantity(2).WithFromDate(this.Transaction.Now().AddDays(-1)).Build();
            this.Transaction.Derive(false);

            var workEffortType = new WorkEffortTypeBuilder(this.Transaction).WithWorkEffortPartStandard(partStandard).Build();
            this.Transaction.Derive(false);

            var agreement = new MaintenanceAgreementBuilder(this.Transaction).WithWorkEffortType(workEffortType).Build();
            this.Transaction.Derive(false);

            var workEffort = new WorkTaskBuilder(this.Transaction).WithMaintenanceAgreement(agreement).WithScheduledStart(this.Transaction.Now()).Build();
            this.Transaction.Derive(false);

            var inventoryAssignment = new WorkEffortInventoryAssignmentBuilder(this.Transaction)
                .WithInventoryItem(part.InventoryItemsWherePart.First())
                .WithAssignment(workEffort)
                .WithQuantity(5)
                .Build();
            this.Transaction.Derive(false);

            partStandard.Quantity = 3;
            this.Transaction.Derive(false);

            Assert.Equal(2, inventoryAssignment.DerivedBillableQuantity);
        }

        [Fact]
        public void ChangedWorkEffortScheduledStartDeriveDerivedBillableQuantity()
        {
            var part = new NonUnifiedPartBuilder(this.Transaction).WithInventoryItemKind(new InventoryItemKinds(this.Transaction).NonSerialised).Build();
            this.Transaction.Derive(false);

            var partStandard = new WorkEffortPartStandardBuilder(this.Transaction).WithPart(part).WithQuantity(2).WithFromDate(this.Transaction.Now().AddDays(-1)).Build();
            this.Transaction.Derive(false);

            var workEffortType = new WorkEffortTypeBuilder(this.Transaction).WithWorkEffortPartStandard(partStandard).Build();
            this.Transaction.Derive(false);

            var agreement = new MaintenanceAgreementBuilder(this.Transaction).WithWorkEffortType(workEffortType).Build();
            this.Transaction.Derive(false);

            var workEffort = new WorkTaskBuilder(this.Transaction).WithMaintenanceAgreement(agreement).WithScheduledStart(this.Transaction.Now().AddDays(-2)).Build();
            this.Transaction.Derive(false);

            var inventoryAssignment = new WorkEffortInventoryAssignmentBuilder(this.Transaction)
                .WithInventoryItem(part.InventoryItemsWherePart.First())
                .WithAssignment(workEffort)
                .WithQuantity(5)
                .Build();
            this.Transaction.Derive(false);

            Assert.Equal(5, inventoryAssignment.DerivedBillableQuantity);

            workEffort.ScheduledStart = this.Transaction.Now();
            this.Transaction.Derive(false);

            Assert.Equal(3, inventoryAssignment.DerivedBillableQuantity);
        }
    }

    public class WorkEffortInventoryAssignmentUnitSellingPriceRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public WorkEffortInventoryAssignmentUnitSellingPriceRuleTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedAssignedUnitSellingPriceDeriveUnitSellingPrice()
        {
            var workEffort = new WorkTaskBuilder(this.Transaction).Build();
            var part = new NonUnifiedPartBuilder(this.Transaction)
                .WithInventoryItemKind(new InventoryItemKinds(this.Transaction).NonSerialised)
                .Build();
            this.Transaction.Derive(false);

            new InventoryItemTransactionBuilder(this.Transaction)
                .WithPart(part)
                .WithReason(new InventoryTransactionReasons(this.Transaction).IncomingShipment)
                .WithQuantity(3)
                .Build();
            this.Transaction.Derive(false);

            var inventoryAssignment = new WorkEffortInventoryAssignmentBuilder(this.Transaction)
                .WithAssignment(workEffort)
                .WithInventoryItem(part.InventoryItemsWherePart.First())
                .WithQuantity(2)
                .Build();
            this.Transaction.Derive(false);

            inventoryAssignment.AssignedUnitSellingPrice = 11;
            this.Transaction.Derive(false);

            Assert.Equal(11, inventoryAssignment.UnitSellingPrice);
        }

        [Fact]
        public void ChangedInventoryItemDeriveUnitSellingPrice()
        {
            var part = new NonUnifiedPartBuilder(this.Transaction).WithInventoryItemKind(new InventoryItemKinds(this.Transaction).NonSerialised).Build();
            new BasePriceBuilder(this.Transaction).WithPricedBy(this.InternalOrganisation).WithProduct(part).WithPrice(11).Build();
            this.Transaction.Derive(false);

            var workEffort = new WorkTaskBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            new InventoryItemTransactionBuilder(this.Transaction)
                .WithPart(part)
                .WithReason(new InventoryTransactionReasons(this.Transaction).IncomingShipment)
                .WithQuantity(3)
                .Build();
            this.Transaction.Derive(false);

            var inventoryAssignment = new WorkEffortInventoryAssignmentBuilder(this.Transaction)
                .WithAssignment(workEffort)
                .WithQuantity(2)
                .WithAssignedUnitSellingPrice(11)
                .WithInventoryItem(part.InventoryItemsWherePart.First())
                .Build();
            this.Transaction.Derive(false);

            Assert.Equal(11, inventoryAssignment.UnitSellingPrice);
        }

        [Fact]
        public void ChangedWorkEffortWorkEffortStateDeriveUnitSellingPrice()
        {
            var part = new NonUnifiedPartBuilder(this.Transaction).WithInventoryItemKind(new InventoryItemKinds(this.Transaction).NonSerialised).Build();
            new BasePriceBuilder(this.Transaction).WithPricedBy(this.InternalOrganisation).WithProduct(part).WithPrice(11).Build();
            this.Transaction.Derive(false);

            var workEffort = new WorkTaskBuilder(this.Transaction).WithWorkEffortState(new WorkEffortStates(this.Transaction).Completed).Build();
            this.Transaction.Derive(false);

            new InventoryItemTransactionBuilder(this.Transaction)
                .WithPart(part)
                .WithReason(new InventoryTransactionReasons(this.Transaction).IncomingShipment)
                .WithQuantity(3)
                .Build();
            this.Transaction.Derive(false);

            var inventoryAssignment = new WorkEffortInventoryAssignmentBuilder(this.Transaction)
                .WithAssignment(workEffort)
                .WithQuantity(2)
                .WithAssignedUnitSellingPrice(11)
                .WithInventoryItem(part.InventoryItemsWherePart.First())
                .Build();
            this.Transaction.Derive(false);

            Assert.Equal(0, inventoryAssignment.UnitSellingPrice);

            workEffort.WorkEffortState = new WorkEffortStates(this.Transaction).Created;
            this.Transaction.Derive(false);

            Assert.Equal(11, inventoryAssignment.UnitSellingPrice);
        }

        [Fact]
        public void ChangedWorkEffortCustomerDeriveUnitSellingPrice()
        {
            this.Transaction.GetSingleton().Settings.InternalPartSurchargePercentage = 25;

            var part = new NonUnifiedPartBuilder(this.Transaction).WithInventoryItemKind(new InventoryItemKinds(this.Transaction).NonSerialised).Build();
            new BasePriceBuilder(this.Transaction).WithPricedBy(this.InternalOrganisation).WithProduct(part).WithPrice(11).Build();
            this.Transaction.Derive(false);

            var workEffort = new WorkTaskBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var inventoryAssignment = new WorkEffortInventoryAssignmentBuilder(this.Transaction)
                .WithAssignment(workEffort)
                .WithQuantity(2)
                .WithUnitPurchasePrice(10)
                .WithInventoryItem(part.InventoryItemsWherePart.First())
                .Build();
            this.Transaction.Derive(false);

            workEffort.Customer = this.InternalOrganisation;
            this.Transaction.Derive(false);

            Assert.Equal(12.5M, inventoryAssignment.UnitSellingPrice);
        }

        [Fact]
        public void ChangedWorkTaskMaintenanceAgreementDeriveUnitSellingPrice()
        {
            var part = new NonUnifiedPartBuilder(this.Transaction).WithInventoryItemKind(new InventoryItemKinds(this.Transaction).NonSerialised).Build();
            this.Transaction.Derive(false);

            var workEffort = new WorkTaskBuilder(this.Transaction).WithScheduledStart(this.Transaction.Now()).Build();
            var inventoryAssignment = new WorkEffortInventoryAssignmentBuilder(this.Transaction)
                .WithInventoryItem(part.InventoryItemsWherePart.First())
                .WithAssignment(workEffort)
                .WithQuantity(1)
                .WithUnitPurchasePrice(10)
                .Build();
            this.Transaction.Derive(false);

            var agreement = new MaintenanceAgreementBuilder(this.Transaction).WithPartSurchargePercentage(10).Build();
            this.Transaction.Derive(false);

            workEffort.MaintenanceAgreement = agreement;
            this.Transaction.Derive(false);

            Assert.Equal(11, inventoryAssignment.UnitSellingPrice);
        }

        [Fact]
        public void ChangedUnitPurchasePriceDeriveUnitSellingPrice()
        {
            var part = new NonUnifiedPartBuilder(this.Transaction).WithInventoryItemKind(new InventoryItemKinds(this.Transaction).NonSerialised).Build();
            this.Transaction.Derive(false);

            var agreement = new MaintenanceAgreementBuilder(this.Transaction).WithPartSurchargePercentage(10).Build();
            this.Transaction.Derive(false);

            var workEffort = new WorkTaskBuilder(this.Transaction).WithMaintenanceAgreement(agreement).WithScheduledStart(this.Transaction.Now()).Build();
            var inventoryAssignment = new WorkEffortInventoryAssignmentBuilder(this.Transaction)
                .WithInventoryItem(part.InventoryItemsWherePart.First())
                .WithAssignment(workEffort)
                .WithQuantity(1)
                .WithUnitPurchasePrice(10)
                .Build();
            this.Transaction.Derive(false);

            inventoryAssignment.UnitPurchasePrice = 20;
            this.Transaction.Derive(false);

            Assert.Equal(22, inventoryAssignment.UnitSellingPrice);
        }

        [Fact]
        public void ChangedMaintenanceAgreementPartSurchargePercentageDeriveUnitSellingPrice()
        {
            var part = new NonUnifiedPartBuilder(this.Transaction).WithInventoryItemKind(new InventoryItemKinds(this.Transaction).NonSerialised).Build();
            this.Transaction.Derive(false);

            var agreement = new MaintenanceAgreementBuilder(this.Transaction).WithPartSurchargePercentage(10).Build();
            this.Transaction.Derive(false);

            var workEffort = new WorkTaskBuilder(this.Transaction).WithMaintenanceAgreement(agreement).WithScheduledStart(this.Transaction.Now()).Build();
            var inventoryAssignment = new WorkEffortInventoryAssignmentBuilder(this.Transaction)
                .WithInventoryItem(part.InventoryItemsWherePart.First())
                .WithAssignment(workEffort)
                .WithQuantity(1)
                .WithUnitPurchasePrice(10)
                .Build();
            this.Transaction.Derive(false);

            agreement.PartSurchargePercentage = 22;
            this.Transaction.Derive(false);

            Assert.Equal(12.2M, inventoryAssignment.UnitSellingPrice);
        }

        [Fact]
        public void ChangedMaintenanceAgreementWorkEffortTypeDeriveUnitSellingPrice()
        {
            var part = new NonUnifiedPartBuilder(this.Transaction).WithInventoryItemKind(new InventoryItemKinds(this.Transaction).NonSerialised).Build();
            this.Transaction.Derive(false);

            var partStandard = new WorkEffortPartStandardBuilder(this.Transaction).WithPart(part).WithQuantity(2).WithFromDate(this.Transaction.Now().AddDays(-1)).Build();
            this.Transaction.Derive(false);

            var workEffortType = new WorkEffortTypeBuilder(this.Transaction).WithWorkEffortPartStandard(partStandard).Build();
            this.Transaction.Derive(false);

            var agreement = new MaintenanceAgreementBuilder(this.Transaction).WithPartSurchargePercentage(10).Build();
            this.Transaction.Derive(false);

            var workEffort = new WorkTaskBuilder(this.Transaction).WithMaintenanceAgreement(agreement).WithScheduledStart(this.Transaction.Now()).Build();
            this.Transaction.Derive(false);

            var inventoryAssignment = new WorkEffortInventoryAssignmentBuilder(this.Transaction)
                .WithInventoryItem(part.InventoryItemsWherePart.First())
                .WithAssignment(workEffort)
                .WithQuantity(5)
                .WithUnitPurchasePrice(10)
                .Build();
            this.Transaction.Derive(false);

            agreement.WorkEffortType = workEffortType;
            this.Transaction.Derive(false);

            Assert.Equal(11, inventoryAssignment.UnitSellingPrice);
        }

        [Fact]
        public void ChangedWorkEffortTypeWorkEffortPartStandardDeriveUnitSellingPrice()
        {
            var part = new NonUnifiedPartBuilder(this.Transaction).WithInventoryItemKind(new InventoryItemKinds(this.Transaction).NonSerialised).Build();
            this.Transaction.Derive(false);

            var partStandard = new WorkEffortPartStandardBuilder(this.Transaction).WithPart(part).WithQuantity(2).WithFromDate(this.Transaction.Now().AddDays(-1)).Build();
            this.Transaction.Derive(false);

            var workEffortType = new WorkEffortTypeBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var agreement = new MaintenanceAgreementBuilder(this.Transaction).WithWorkEffortType(workEffortType).WithPartSurchargePercentage(10).Build();
            this.Transaction.Derive(false);

            var workEffort = new WorkTaskBuilder(this.Transaction).WithMaintenanceAgreement(agreement).WithScheduledStart(this.Transaction.Now()).Build();
            this.Transaction.Derive(false);

            var inventoryAssignment = new WorkEffortInventoryAssignmentBuilder(this.Transaction)
                .WithInventoryItem(part.InventoryItemsWherePart.First())
                .WithAssignment(workEffort)
                .WithQuantity(5)
                .WithUnitPurchasePrice(10)
                .Build();
            this.Transaction.Derive(false);

            workEffortType.AddWorkEffortPartStandard(partStandard);
            this.Transaction.Derive(false);

            Assert.Equal(11, inventoryAssignment.UnitSellingPrice);
        }

        [Fact]
        public void ChangedWorkEffortPartStandardFromDateDeriveUnitSellingPrice()
        {
            var part = new NonUnifiedPartBuilder(this.Transaction).WithInventoryItemKind(new InventoryItemKinds(this.Transaction).NonSerialised).Build();
            this.Transaction.Derive(false);

            var partStandard = new WorkEffortPartStandardBuilder(this.Transaction).WithPart(part).WithQuantity(2).WithFromDate(this.Transaction.Now().AddDays(1)).Build();
            this.Transaction.Derive(false);

            var workEffortType = new WorkEffortTypeBuilder(this.Transaction).WithWorkEffortPartStandard(partStandard).Build();
            this.Transaction.Derive(false);

            var agreement = new MaintenanceAgreementBuilder(this.Transaction).WithWorkEffortType(workEffortType).WithPartSurchargePercentage(10).Build();
            this.Transaction.Derive(false);

            var workEffort = new WorkTaskBuilder(this.Transaction).WithMaintenanceAgreement(agreement).WithScheduledStart(this.Transaction.Now()).Build();
            this.Transaction.Derive(false);

            var inventoryAssignment = new WorkEffortInventoryAssignmentBuilder(this.Transaction)
                .WithInventoryItem(part.InventoryItemsWherePart.First())
                .WithAssignment(workEffort)
                .WithQuantity(5)
                .WithUnitPurchasePrice(10)
                .Build();
            this.Transaction.Derive(false);

            partStandard.FromDate = this.Transaction.Now().AddDays(-1);
            this.Transaction.Derive(false);

            Assert.Equal(11, inventoryAssignment.UnitSellingPrice);
        }

        [Fact]
        public void ChangedWorkEffortPartStandardThroughDateDeriveUnitSellingPrice()
        {
            var part = new NonUnifiedPartBuilder(this.Transaction).WithInventoryItemKind(new InventoryItemKinds(this.Transaction).NonSerialised).Build();
            this.Transaction.Derive(false);

            var partStandard = new WorkEffortPartStandardBuilder(this.Transaction).WithPart(part).WithQuantity(2).WithFromDate(this.Transaction.Now().AddDays(-1)).WithThroughDate(this.Transaction.Now().AddDays(-1)).Build();
            this.Transaction.Derive(false);

            var workEffortType = new WorkEffortTypeBuilder(this.Transaction).WithWorkEffortPartStandard(partStandard).Build();
            this.Transaction.Derive(false);

            var agreement = new MaintenanceAgreementBuilder(this.Transaction).WithWorkEffortType(workEffortType).WithPartSurchargePercentage(10).Build();
            this.Transaction.Derive(false);

            var workEffort = new WorkTaskBuilder(this.Transaction).WithMaintenanceAgreement(agreement).WithScheduledStart(this.Transaction.Now()).Build();
            this.Transaction.Derive(false);

            var inventoryAssignment = new WorkEffortInventoryAssignmentBuilder(this.Transaction)
                .WithInventoryItem(part.InventoryItemsWherePart.First())
                .WithAssignment(workEffort)
                .WithQuantity(5)
                .WithUnitPurchasePrice(10)
                .Build();
            this.Transaction.Derive(false);

            partStandard.RemoveThroughDate();
            this.Transaction.Derive(false);

            Assert.Equal(11, inventoryAssignment.UnitSellingPrice);
        }

        [Fact]
        public void ChangedWorkEffortPartStandardQuantityDeriveUnitSellingPrice()
        {
            var part = new NonUnifiedPartBuilder(this.Transaction).WithInventoryItemKind(new InventoryItemKinds(this.Transaction).NonSerialised).Build();
            this.Transaction.Derive(false);

            var partStandard = new WorkEffortPartStandardBuilder(this.Transaction).WithPart(part).WithQuantity(2).WithFromDate(this.Transaction.Now().AddDays(-1)).Build();
            this.Transaction.Derive(false);

            var workEffortType = new WorkEffortTypeBuilder(this.Transaction).WithWorkEffortPartStandard(partStandard).Build();
            this.Transaction.Derive(false);

            var agreement = new MaintenanceAgreementBuilder(this.Transaction).WithWorkEffortType(workEffortType).WithPartSurchargePercentage(10).Build();
            this.Transaction.Derive(false);

            var workEffort = new WorkTaskBuilder(this.Transaction).WithMaintenanceAgreement(agreement).WithScheduledStart(this.Transaction.Now()).Build();
            this.Transaction.Derive(false);

            var inventoryAssignment = new WorkEffortInventoryAssignmentBuilder(this.Transaction)
                .WithInventoryItem(part.InventoryItemsWherePart.First())
                .WithAssignment(workEffort)
                .WithQuantity(1)
                .WithUnitPurchasePrice(10)
                .Build();
            this.Transaction.Derive(false);

            partStandard.Quantity = 3;
            this.Transaction.Derive(false);

            Assert.Equal(0, inventoryAssignment.UnitSellingPrice);
        }

        [Fact]
        public void ChangedWorkEffortScheduledStartDeriveUnitSellingPrice()
        {
            var part = new NonUnifiedPartBuilder(this.Transaction).WithInventoryItemKind(new InventoryItemKinds(this.Transaction).NonSerialised).Build();
            this.Transaction.Derive(false);

            var partStandard = new WorkEffortPartStandardBuilder(this.Transaction).WithPart(part).WithQuantity(2).WithFromDate(this.Transaction.Now().AddDays(-1)).Build();
            this.Transaction.Derive(false);

            var workEffortType = new WorkEffortTypeBuilder(this.Transaction).WithWorkEffortPartStandard(partStandard).Build();
            this.Transaction.Derive(false);

            var agreement = new MaintenanceAgreementBuilder(this.Transaction).WithWorkEffortType(workEffortType).WithPartSurchargePercentage(10).Build();
            this.Transaction.Derive(false);

            var workEffort = new WorkTaskBuilder(this.Transaction).WithMaintenanceAgreement(agreement).WithScheduledStart(this.Transaction.Now().AddDays(-2)).Build();
            this.Transaction.Derive(false);

            var inventoryAssignment = new WorkEffortInventoryAssignmentBuilder(this.Transaction)
                .WithInventoryItem(part.InventoryItemsWherePart.First())
                .WithAssignment(workEffort)
                .WithQuantity(5)
                .WithUnitPurchasePrice(10)
                .Build();
            this.Transaction.Derive(false);

            workEffort.ScheduledStart = this.Transaction.Now();
            this.Transaction.Derive(false);

            Assert.Equal(11, inventoryAssignment.UnitSellingPrice);
        }
    }
}
