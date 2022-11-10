// <copyright file="VehicleTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

using Xunit;

namespace Allors.Database.Domain.Tests
{
    [Trait("Category", "Security")]
    public class VehicleDeniedPermissionRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public VehicleDeniedPermissionRuleTests(Fixture fixture) : base(fixture) => this.deleteRevocation = new Revocations(this.Transaction).VehicleDeleteRevocation;

        public override Config Config => new Config { SetupSecurity = true };

        private readonly Revocation deleteRevocation;

        [Fact]
        public void OnChangedWorkTaskVehicleDeriveDeletePermissionAllowed()
        {
            var vehicle = new VehicleBuilder(this.Transaction).Build();
            var worktask = new WorkTaskBuilder(this.Transaction).Build();
            var assignment = new WorkEffortFixedAssetAssignmentBuilder(this.Transaction).WithAssignment(worktask).WithFixedAsset(vehicle).Build();
            this.Transaction.Derive(false);

            assignment.RemoveFixedAsset();
            this.Transaction.Derive(false);

            Assert.DoesNotContain(this.deleteRevocation, vehicle.Revocations);
        }

        [Fact]
        public void OnChangedWorkTaskVehicleDeriveDeletePermissionDenied()
        {
            var vehicle = new VehicleBuilder(this.Transaction).Build();
            var worktask = new WorkTaskBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var assignment = new WorkEffortFixedAssetAssignmentBuilder(this.Transaction).WithAssignment(worktask).WithFixedAsset(vehicle).Build();
            this.Transaction.Derive(false);

            Assert.Contains(this.deleteRevocation, vehicle.Revocations);
        }
    }
}
