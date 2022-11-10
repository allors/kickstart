// <copyright file="WorkEffortFixedAssetAssignmentTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain.Tests
{
    using System.Linq;
    using Allors;
    using Xunit;

    public class WorkEffortFixedAssetAssignmentSecurityTests : DomainTest, IClassFixture<Fixture>
    {
        public WorkEffortFixedAssetAssignmentSecurityTests(Fixture fixture) : base(fixture) { }

        public override Config Config => new Config { SetupSecurity = true };

        [Fact]
        public void WorkTaskInProgress()
        {
            var workTask = new WorkTaskBuilder(this.Transaction).WithTakenBy(this.InternalOrganisation).Build();
            this.Transaction.Derive(false);

            var timeEntry = new TimeEntryBuilder(this.Transaction)
                .WithRateType(new RateTypes(this.Transaction).StandardRate)
                .WithFromDate(this.Transaction.Now())
                .WithWorkEffort(workTask)
                .Build();

            this.Employee.TimeSheetWhereWorker.AddTimeEntry(timeEntry);
            this.Transaction.Derive(false);

            Assert.Equal(workTask.WorkEffortState, new WorkEffortStates(this.Transaction).InProgress);

            var workEffortFixedAssetAssignment = new WorkEffortFixedAssetAssignmentBuilder(this.Transaction)
                .WithAssignment(workTask)
                .Build();
            this.Transaction.Derive(false);

            this.Transaction.SetUser(this.Administrator);

            var acl = new DatabaseAccessControl(this.Security, this.Administrator)[workEffortFixedAssetAssignment];
            Assert.True(acl.CanWrite(M.WorkEffortFixedAssetAssignment.FixedAsset));
        }
    }
}
