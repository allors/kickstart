using Allors.Database.Derivations;
using Allors.Database.Domain.TestPopulation;
using Resources;
using System.Collections.Generic;
using Xunit;

namespace Allors.Database.Domain.Tests
{
    public class WorkEffortFixedAssetAssignmentTests : DomainTest, IClassFixture<Fixture>
    {
        public WorkEffortFixedAssetAssignmentTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedAssignmentThrowValidationError()
        {
            var part1 = new UnifiedGoodBuilder(this.Transaction).WithInventoryItemKind(new InventoryItemKinds(this.Transaction).Serialised).Build();
            var part2 = new UnifiedGoodBuilder(this.Transaction).WithInventoryItemKind(new InventoryItemKinds(this.Transaction).Serialised).Build();
            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            part1.AddSerialisedItem(serialisedItem);

            var workEffortType = new WorkEffortTypeBuilder(this.Transaction)
                .WithUnifiedGood(part2)
                .Build();
            var agreeement = new MaintenanceAgreementBuilder(this.Transaction).WithWorkEffortType(workEffortType).Build();

            this.Transaction.Derive(false);

            var worktask = new WorkTaskBuilder(this.Transaction).WithMaintenanceAgreement(agreeement).Build();
            this.Transaction.Derive(false);

            var workEffortFixedAssetAssignment = new WorkEffortFixedAssetAssignmentBuilder(this.Transaction)
                .WithFixedAsset(serialisedItem)
                .Build();
            this.Transaction.Derive(false);

            workEffortFixedAssetAssignment.Assignment = worktask;

            var expectedMessage = $"{workEffortFixedAssetAssignment}, { this.M.WorkEffortFixedAssetAssignment.FixedAsset}, { ErrorMessages.SerialisedItemNotInMaintenanceAgreement}";
            var errors = new List<IDerivationError>(this.Transaction.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.Equals(expectedMessage));
        }

        [Fact]
        public void ChangedWorkEffortExistMaintenanceAgreementThrowValidationError()
        {
            var part1 = new UnifiedGoodBuilder(this.Transaction).WithInventoryItemKind(new InventoryItemKinds(this.Transaction).Serialised).Build();
            var part2 = new UnifiedGoodBuilder(this.Transaction).WithInventoryItemKind(new InventoryItemKinds(this.Transaction).Serialised).Build();
            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            part1.AddSerialisedItem(serialisedItem);

            var workEffortType = new WorkEffortTypeBuilder(this.Transaction)
                .WithUnifiedGood(part2)
                .Build();
            var agreeement = new MaintenanceAgreementBuilder(this.Transaction).WithWorkEffortType(workEffortType).Build();

            this.Transaction.Derive(false);

            var worktask = new WorkTaskBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var workEffortFixedAssetAssignment = new WorkEffortFixedAssetAssignmentBuilder(this.Transaction)
                .WithFixedAsset(serialisedItem)
                .WithAssignment(worktask)
                .Build();
            this.Transaction.Derive(false);

            worktask.MaintenanceAgreement = agreeement;

            var expectedMessage = $"{workEffortFixedAssetAssignment}, { this.M.WorkEffortFixedAssetAssignment.FixedAsset}, { ErrorMessages.SerialisedItemNotInMaintenanceAgreement}";
            var errors = new List<IDerivationError>(this.Transaction.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.Equals(expectedMessage));
        }

        [Fact]
        public void ChangedMaintenanceAgreementWorkEffortTypeThrowValidationError()
        {
            var part1 = new UnifiedGoodBuilder(this.Transaction).WithInventoryItemKind(new InventoryItemKinds(this.Transaction).Serialised).Build();
            var part2 = new UnifiedGoodBuilder(this.Transaction).WithInventoryItemKind(new InventoryItemKinds(this.Transaction).Serialised).Build();
            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            part1.AddSerialisedItem(serialisedItem);

            var workEffortType = new WorkEffortTypeBuilder(this.Transaction)
                .WithUnifiedGood(part1)
                .Build();
            var agreeement = new MaintenanceAgreementBuilder(this.Transaction).WithWorkEffortType(workEffortType).Build();

            this.Transaction.Derive(false);

            var worktask = new WorkTaskBuilder(this.Transaction).WithMaintenanceAgreement(agreeement).Build();
            this.Transaction.Derive(false);

            var workEffortFixedAssetAssignment = new WorkEffortFixedAssetAssignmentBuilder(this.Transaction)
                .WithFixedAsset(serialisedItem)
                .WithAssignment(worktask)
                .Build();
            this.Transaction.Derive(false);

            var workEffortType2 = new WorkEffortTypeBuilder(this.Transaction).WithUnifiedGood(part2).Build();
            this.Transaction.Derive(false);

            agreeement.WorkEffortType = workEffortType2;

            var expectedMessage = $"{workEffortFixedAssetAssignment}, { this.M.WorkEffortFixedAssetAssignment.FixedAsset}, { ErrorMessages.SerialisedItemNotInMaintenanceAgreement}";
            var errors = new List<IDerivationError>(this.Transaction.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.Equals(expectedMessage));
        }

        [Fact]
        public void ChangedWorkEffortTypeUnifiedGoodThrowValidationError()
        {
            var part1 = new UnifiedGoodBuilder(this.Transaction).WithInventoryItemKind(new InventoryItemKinds(this.Transaction).Serialised).Build();
            var part2 = new UnifiedGoodBuilder(this.Transaction).WithInventoryItemKind(new InventoryItemKinds(this.Transaction).Serialised).Build();
            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            part1.AddSerialisedItem(serialisedItem);

            var workEffortType = new WorkEffortTypeBuilder(this.Transaction)
                .WithUnifiedGood(part1)
                .Build();
            var agreeement = new MaintenanceAgreementBuilder(this.Transaction).WithWorkEffortType(workEffortType).Build();

            this.Transaction.Derive(false);

            var worktask = new WorkTaskBuilder(this.Transaction).WithMaintenanceAgreement(agreeement).Build();
            this.Transaction.Derive(false);

            var workEffortFixedAssetAssignment = new WorkEffortFixedAssetAssignmentBuilder(this.Transaction)
                .WithFixedAsset(serialisedItem)
                .WithAssignment(worktask)
                .Build();
            this.Transaction.Derive(false);

            workEffortType.UnifiedGood = part2;

            var expectedMessage = $"{workEffortFixedAssetAssignment}, { this.M.WorkEffortFixedAssetAssignment.FixedAsset}, { ErrorMessages.SerialisedItemNotInMaintenanceAgreement}";
            var errors = new List<IDerivationError>(this.Transaction.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.Equals(expectedMessage));
        }

        [Fact]
        public void ChangedFixedAssetThrowValidationError()
        {
            var part1 = new UnifiedGoodBuilder(this.Transaction).WithInventoryItemKind(new InventoryItemKinds(this.Transaction).Serialised).Build();
            var part2 = new UnifiedGoodBuilder(this.Transaction).WithInventoryItemKind(new InventoryItemKinds(this.Transaction).Serialised).Build();
            var serialisedItem1 = new SerialisedItemBuilder(this.Transaction).Build();
            part1.AddSerialisedItem(serialisedItem1);
            var serialisedItem2 = new SerialisedItemBuilder(this.Transaction).Build();
            part2.AddSerialisedItem(serialisedItem2);

            var workEffortType = new WorkEffortTypeBuilder(this.Transaction)
                .WithUnifiedGood(part1)
                .Build();
            var agreeement = new MaintenanceAgreementBuilder(this.Transaction).WithWorkEffortType(workEffortType).Build();

            this.Transaction.Derive(false);

            var worktask = new WorkTaskBuilder(this.Transaction).WithMaintenanceAgreement(agreeement).Build();
            this.Transaction.Derive(false);

            var workEffortFixedAssetAssignment = new WorkEffortFixedAssetAssignmentBuilder(this.Transaction)
                .WithFixedAsset(serialisedItem1)
                .WithAssignment(worktask)
                .Build();
            this.Transaction.Derive(false);

            workEffortFixedAssetAssignment.FixedAsset = serialisedItem2;

            var expectedMessage = $"{workEffortFixedAssetAssignment}, { this.M.WorkEffortFixedAssetAssignment.FixedAsset}, { ErrorMessages.SerialisedItemNotInMaintenanceAgreement}";
            var errors = new List<IDerivationError>(this.Transaction.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.Equals(expectedMessage));
        }
    }
}
