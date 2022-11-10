// <copyright file="BlueCollarWorkerTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

using Allors.Database.Domain.Tests;

namespace Allors.Database.Domain.Tests
{
    using System.Linq;
    using Xunit;

    public class BlueCollarWorkerTests : DomainTest, IClassFixture<Fixture>
    {
        public BlueCollarWorkerTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void UserGroup()
        {
            var organisation = new OrganisationBuilder(this.Transaction).WithName("organisation").WithIsInternalOrganisation(true).Build();
            this.Transaction.Derive(true);

            Assert.True(organisation.ExistBlueCollarWorkerUserGroup);
            
            organisation.RemoveBlueCollarWorkerUserGroup();
            this.Transaction.Derive();

            Assert.True(organisation.ExistBlueCollarWorkerUserGroup);
        }

        [Fact]
        public void Grant()
        {
            var organisation = new OrganisationBuilder(this.Transaction).WithName("organisation").WithIsInternalOrganisation(true).Build();
            this.Transaction.Derive(true);

            Assert.True(organisation.ExistBlueCollarWorkerGrant);
            Assert.Equal(new Roles(this.Transaction).BlueCollarWorker, organisation.BlueCollarWorkerGrant.Role);
            Assert.Contains(organisation.BlueCollarWorkerUserGroup, organisation.BlueCollarWorkerGrant.SubjectGroups);

            organisation.RemoveBlueCollarWorkerGrant();

            this.Transaction.Derive(true);

            Assert.True(organisation.ExistBlueCollarWorkerGrant);
            Assert.Equal(new Roles(this.Transaction).BlueCollarWorker, organisation.BlueCollarWorkerGrant.Role);
            Assert.Contains(organisation.BlueCollarWorkerUserGroup, organisation.BlueCollarWorkerGrant.SubjectGroups);
        }

        [Fact]
        public void SecurityToken()
        {
            var organisation = new OrganisationBuilder(this.Transaction).WithName("organisation").WithIsInternalOrganisation(true).Build();
            this.Transaction.Derive(true);

            Assert.True(organisation.ExistBlueCollarWorkerSecurityToken);
            Assert.Contains(organisation.BlueCollarWorkerGrant, organisation.BlueCollarWorkerSecurityToken.Grants);
        }

        [Fact]
        public void BlueCollarWorkers()
        {
            var organisation = new OrganisationBuilder(this.Transaction).WithName("organisation").WithIsInternalOrganisation(true).Build();
            this.Transaction.Derive(true);

            var worker = new PersonBuilder(this.Transaction)
                .WithUserName("worker")
                .WithFirstName("blue-collar")
                .WithLastName("worker")
                .Build();

            organisation.AddBlueCollarWorker(worker);

            this.Transaction.Derive(true);

            Assert.Contains(worker, organisation.BlueCollarWorkerUserGroup.Members);
        }
    }

    public class BlueCollarWorkerSecurityTests : DomainTest, IClassFixture<Fixture>
    {
        public BlueCollarWorkerSecurityTests(Fixture fixture) : base(fixture) { }

        public override Config Config => new Config { SetupSecurity = true };

        [Fact]
        public void WorkEffortInventoryAssignmentOwnInternalOrganisation()
        {
            var worker = new PersonBuilder(this.Transaction)
                .WithUserName("worker")
                .WithFirstName("blue-collar")
                .WithLastName("worker")
                .Build();

            this.InternalOrganisation.AddBlueCollarWorker(worker);

            this.Transaction.Derive(true);

            var customer = new OrganisationBuilder(this.Transaction).WithName("Org1").Build();
            new CustomerRelationshipBuilder(this.Transaction).WithCustomer(customer).WithInternalOrganisation(this.InternalOrganisation).Build();

            var workTask = new WorkTaskBuilder(this.Transaction).WithName("Activity").WithCustomer(customer).WithTakenBy(this.InternalOrganisation).Build();

            this.Transaction.Derive();

            var part = new NonUnifiedPartBuilder(this.Transaction)
                .WithProductIdentification(new PartNumberBuilder(this.Transaction)
                    .WithIdentification("P1")
                    .WithProductIdentificationType(new ProductIdentificationTypes(this.Transaction).Part).Build())
                .Build();

            this.Transaction.Derive(true);

            new InventoryItemTransactionBuilder(this.Transaction).WithQuantity(100).WithReason(new InventoryTransactionReasons(this.Transaction).Unknown).WithPart(part).Build();
            this.Transaction.Derive(true);

            var inventoryAssignment = new WorkEffortInventoryAssignmentBuilder(this.Transaction)
                .WithAssignment(workTask)
                .WithInventoryItem(part.InventoryItemsWherePart.First())
                .WithQuantity(10)
                .Build();

            this.Transaction.Derive(true);
            this.Transaction.Commit();

            Assert.Equal(new WorkEffortStates(this.Transaction).Created, workTask.WorkEffortState);

            this.Transaction.SetUser(worker);

            var acl = new DatabaseAccessControl(this.Security, worker)[inventoryAssignment];
            Assert.True(acl.CanRead(M.WorkEffortInventoryAssignment.InventoryItem));
            Assert.False(acl.CanWrite(M.WorkEffortInventoryAssignment.InventoryItem));
            Assert.True(acl.CanRead(M.WorkEffortInventoryAssignment.Quantity));
            Assert.False(acl.CanWrite(M.WorkEffortInventoryAssignment.Quantity));
            Assert.False(acl.CanRead(M.WorkEffortInventoryAssignment.AssignedBillableQuantity));
            Assert.False(acl.CanWrite(M.WorkEffortInventoryAssignment.AssignedBillableQuantity));
            Assert.False(acl.CanRead(M.WorkEffortInventoryAssignment.UnitSellingPrice));
            Assert.False(acl.CanWrite(M.WorkEffortInventoryAssignment.UnitSellingPrice));
            Assert.False(acl.CanRead(M.WorkEffortInventoryAssignment.AssignedUnitSellingPrice));
            Assert.False(acl.CanWrite(M.WorkEffortInventoryAssignment.AssignedUnitSellingPrice));
            Assert.False(acl.CanRead(M.WorkEffortInventoryAssignment.UnitPurchasePrice));
            Assert.False(acl.CanWrite(M.WorkEffortInventoryAssignment.UnitPurchasePrice));
        }

        [Fact]
        public void WorkEffortInventoryAssignmentOtherInternalOrganisation()
        {
            var worker = new PersonBuilder(this.Transaction)
                .WithUserName("worker")
                .WithFirstName("blue-collar")
                .WithLastName("worker")
                .Build();

            this.InternalOrganisation.AddBlueCollarWorker(worker);

            this.Transaction.Derive(true);

            var otherInternalOrganisation = new OrganisationBuilder(this.Transaction).WithIsInternalOrganisation(true).WithName("other internalOrganisation").Build();
            this.Transaction.Derive(true);

            var customer = new OrganisationBuilder(this.Transaction).WithName("Org1").Build();
            new CustomerRelationshipBuilder(this.Transaction).WithCustomer(customer).WithInternalOrganisation(otherInternalOrganisation).Build();

            var workTask = new WorkTaskBuilder(this.Transaction).WithName("Activity").WithCustomer(customer).WithTakenBy(otherInternalOrganisation).Build();

            this.Transaction.Derive();

            var part = new NonUnifiedPartBuilder(this.Transaction)
                .WithProductIdentification(new PartNumberBuilder(this.Transaction)
                    .WithIdentification("P1")
                    .WithProductIdentificationType(new ProductIdentificationTypes(this.Transaction).Part).Build())
                .Build();

            this.Transaction.Derive(true);

            new InventoryItemTransactionBuilder(this.Transaction).WithQuantity(100).WithReason(new InventoryTransactionReasons(this.Transaction).Unknown).WithPart(part).Build();
            this.Transaction.Derive(true);

            var inventoryAssignment = new WorkEffortInventoryAssignmentBuilder(this.Transaction)
                .WithAssignment(workTask)
                .WithInventoryItem(part.InventoryItemsWherePart.First())
                .WithQuantity(10)
                .Build();

            this.Transaction.Derive(true);
            this.Transaction.Commit();

            Assert.Equal(new WorkEffortStates(this.Transaction).Created, workTask.WorkEffortState);

            this.Transaction.SetUser(worker);

            var acl = new DatabaseAccessControl(this.Security, worker)[inventoryAssignment];
            Assert.False(acl.CanRead(M.WorkEffortInventoryAssignment.InventoryItem));
            Assert.False(acl.CanWrite(M.WorkEffortInventoryAssignment.InventoryItem));
            Assert.False(acl.CanRead(M.WorkEffortInventoryAssignment.Quantity));
            Assert.False(acl.CanWrite(M.WorkEffortInventoryAssignment.Quantity));
            Assert.False(acl.CanRead(M.WorkEffortInventoryAssignment.AssignedBillableQuantity));
            Assert.False(acl.CanWrite(M.WorkEffortInventoryAssignment.AssignedBillableQuantity));
            Assert.False(acl.CanRead(M.WorkEffortInventoryAssignment.UnitSellingPrice));
            Assert.False(acl.CanWrite(M.WorkEffortInventoryAssignment.UnitSellingPrice));
            Assert.False(acl.CanRead(M.WorkEffortInventoryAssignment.AssignedUnitSellingPrice));
            Assert.False(acl.CanWrite(M.WorkEffortInventoryAssignment.AssignedUnitSellingPrice));
            Assert.False(acl.CanRead(M.WorkEffortInventoryAssignment.UnitPurchasePrice));
            Assert.False(acl.CanWrite(M.WorkEffortInventoryAssignment.UnitPurchasePrice));
        }

        [Fact]
        public void WorkTaskOwnInternalOrganisation()
        {
            var worker = new PersonBuilder(this.Transaction)
                .WithUserName("worker")
                .WithFirstName("blue-collar")
                .WithLastName("worker")
                .Build();

            this.InternalOrganisation.AddBlueCollarWorker(worker);
            this.Transaction.Derive(true);

            var customer = new OrganisationBuilder(this.Transaction).WithName("Org1").Build();
            new CustomerRelationshipBuilder(this.Transaction).WithCustomer(customer).WithInternalOrganisation(this.InternalOrganisation).Build();

            var workTask = new WorkTaskBuilder(this.Transaction).WithName("Activity").WithCustomer(customer).WithTakenBy(this.InternalOrganisation).Build();

            this.Transaction.Derive();
            this.Transaction.Commit();

            this.Transaction.SetUser(worker);

            var acl = new DatabaseAccessControl(this.Security, worker)[workTask];
            Assert.True(acl.CanRead(M.WorkTask.WorkDone));
            Assert.True(acl.CanWrite(M.WorkTask.WorkDone));
        }

        [Fact]
        public void WorkTaskOtherInternalOrganisation()
        {
            var worker = new PersonBuilder(this.Transaction)
                .WithUserName("worker")
                .WithFirstName("blue-collar")
                .WithLastName("worker")
                .Build();

            this.InternalOrganisation.AddBlueCollarWorker(worker);
            this.Transaction.Derive(true);

            var otherInternalOrganisation = new OrganisationBuilder(this.Transaction).WithIsInternalOrganisation(true).WithName("other internalOrganisation").Build();
            this.Transaction.Derive(true);

            var customer = new OrganisationBuilder(this.Transaction).WithName("Org1").Build();
            new CustomerRelationshipBuilder(this.Transaction).WithCustomer(customer).WithInternalOrganisation(otherInternalOrganisation).Build();

            var workTask = new WorkTaskBuilder(this.Transaction).WithName("Activity").WithCustomer(customer).WithTakenBy(otherInternalOrganisation).Build();

            this.Transaction.Derive();
            this.Transaction.Commit();

            this.Transaction.SetUser(worker);

            var acl = new DatabaseAccessControl(this.Security, worker)[workTask];
            Assert.False(acl.CanRead(M.WorkTask.WorkDone));
            Assert.False(acl.CanWrite(M.WorkTask.WorkDone));
        }

        [Fact]
        public void TimeEntryOwnInternalOrganisation()
        {
            var worker = new PersonBuilder(this.Transaction)
                .WithUserName("worker")
                .WithFirstName("blue-collar")
                .WithLastName("worker")
                .Build();

            this.InternalOrganisation.AddBlueCollarWorker(worker);
            new EmploymentBuilder(this.Transaction).WithEmployer(this.InternalOrganisation).WithEmployee(worker).Build();

            this.Transaction.Derive(true);

            var customer = new OrganisationBuilder(this.Transaction).WithName("Org1").Build();
            new CustomerRelationshipBuilder(this.Transaction).WithCustomer(customer).WithInternalOrganisation(this.InternalOrganisation).Build();

            var workTask = new WorkTaskBuilder(this.Transaction).WithName("Activity").WithCustomer(customer).WithTakenBy(this.InternalOrganisation).Build();

            var timeEntry = new TimeEntryBuilder(this.Transaction)
                .WithRateType(new RateTypes(this.Transaction).StandardRate)
                .WithFromDate(this.Transaction.Now())
                .WithWorkEffort(workTask)
                .Build();

            worker.TimeSheetWhereWorker.AddTimeEntry(timeEntry);

            this.Transaction.Derive();
            this.Transaction.Commit();

            this.Transaction.SetUser(worker);

            var acl = new DatabaseAccessControl(this.Security, worker)[timeEntry];
            Assert.True(acl.CanRead(M.TimeEntry.ThroughDate));
            Assert.True(acl.CanWrite(M.TimeEntry.ThroughDate));
        }

        [Fact(Skip = "TODO: Martien")]
        public void TimeEntryOtherInternalOrganisation()
        {
            var worker = new PersonBuilder(this.Transaction)
                .WithUserName("worker")
                .WithFirstName("blue-collar")
                .WithLastName("worker")
                .Build();

            this.InternalOrganisation.AddBlueCollarWorker(worker);
            new EmploymentBuilder(this.Transaction).WithEmployer(this.InternalOrganisation).WithEmployee(worker).Build();

            this.Transaction.Derive(true);

            var otherInternalOrganisation = new OrganisationBuilder(this.Transaction).WithIsInternalOrganisation(true).WithName("other internalOrganisation").Build();
            this.Transaction.Derive(true);

            var anotherWorker = new PersonBuilder(this.Transaction)
                .WithUserName("worker for other internalOrganisation")
                .WithFirstName("another blue-collar")
                .WithLastName("worker")
                .Build();

            otherInternalOrganisation.AddBlueCollarWorker(anotherWorker);
            new EmploymentBuilder(this.Transaction).WithEmployer(otherInternalOrganisation).WithEmployee(anotherWorker).Build();

            this.Transaction.Derive();

            var customer = new OrganisationBuilder(this.Transaction).WithName("Org1").Build();
            new CustomerRelationshipBuilder(this.Transaction).WithCustomer(customer).WithInternalOrganisation(otherInternalOrganisation).Build();

            var workTask = new WorkTaskBuilder(this.Transaction).WithName("Activity").WithCustomer(customer).WithTakenBy(otherInternalOrganisation).Build();

            this.Transaction.SetUser(anotherWorker);

            var timeEntry = new TimeEntryBuilder(this.Transaction)
                .WithRateType(new RateTypes(this.Transaction).StandardRate)
                .WithFromDate(this.Transaction.Now())
                .WithWorkEffort(workTask)
                .Build();

            anotherWorker.TimeSheetWhereWorker.AddTimeEntry(timeEntry);

            this.Transaction.Derive();
            this.Transaction.Commit();

            this.Transaction.SetUser(worker);

            var acl = new DatabaseAccessControl(this.Security, worker)[timeEntry];
            Assert.False(acl.CanRead(M.TimeEntry.ThroughDate));
            Assert.False(acl.CanWrite(M.TimeEntry.ThroughDate));
        }
    }
}
