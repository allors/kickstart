// <copyright file="WorkTask.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain.Tests
{
    using System.Linq;
    using Allors;
    using Xunit;

    public class WorkTaskSecurityTests : DomainTest, IClassFixture<Fixture>
    {
        public WorkTaskSecurityTests(Fixture fixture) : base(fixture) { }

        public override Config Config => new Config { SetupSecurity = true };

        [Fact]
        public void AddDocumentClosedWorkTaskOwnInternalOrganisation()
        {
            var internalOrganisation = new Organisations(this.Transaction).Extent().First(o => o.IsInternalOrganisation);

            var localAdmin = new PersonBuilder(this.Transaction)
                .WithUserName("localadmin")
                .WithLastName("localAdmin")
                .Build();

            internalOrganisation.AddLocalAdministrator(localAdmin);
            new EmploymentBuilder(this.Transaction).WithEmployer(internalOrganisation).WithEmployee(localAdmin).Build();

            var userGroups = new UserGroups(this.Transaction);
            userGroups.Creators.AddMember(localAdmin);
            userGroups.LocalAdministratorsGlobal.AddMember(localAdmin);

            this.Transaction.Derive(true);

            var customer = new OrganisationBuilder(this.Transaction).WithName("Org1").Build();
            new CustomerRelationshipBuilder(this.Transaction).WithCustomer(customer).WithInternalOrganisation(internalOrganisation).Build();

            var workTask = new WorkTaskBuilder(this.Transaction).WithName("Activity").WithCustomer(customer).WithTakenBy(internalOrganisation).Build();

            this.Transaction.Derive();
            this.Transaction.Commit();

            var timeEntry = new TimeEntryBuilder(this.Transaction)
                .WithRateType(new RateTypes(this.Transaction).StandardRate)
                .WithFromDate(this.Transaction.Now())
                .WithWorkEffort(workTask)
                .Build();

            localAdmin.TimeSheetWhereWorker.AddTimeEntry(timeEntry);

            this.Transaction.Derive();
            this.Transaction.Commit();

            workTask.Complete();

            this.Transaction.SetUser(localAdmin);

            Assert.Equal(workTask.WorkEffortState, new WorkEffortStates(this.Transaction).Completed);

            var acl = new DatabaseAccessControl(this.Security, localAdmin)[workTask];
            Assert.True(acl.CanRead(M.WorkTask.PublicElectronicDocuments));
            Assert.True(acl.CanWrite(M.WorkTask.PublicElectronicDocuments));
        }

        [Fact]
        public void ReviseFinishedWorkTaskForInternalWork()
        {
            var internalOrganisation = new Organisations(this.Transaction).Extent().First(o => o.IsInternalOrganisation);

            var localAdmin = new PersonBuilder(this.Transaction)
                .WithUserName("localadmin")
                .WithLastName("localAdmin")
                .Build();

            internalOrganisation.AddLocalAdministrator(localAdmin);
            new EmploymentBuilder(this.Transaction).WithEmployer(internalOrganisation).WithEmployee(localAdmin).Build();

            var userGroups = new UserGroups(this.Transaction);
            userGroups.Creators.AddMember(localAdmin);
            userGroups.LocalAdministratorsGlobal.AddMember(localAdmin);

            this.Transaction.Derive(true);

            var workTask = new WorkTaskBuilder(this.Transaction).WithName("Activity")
                .WithCustomer(this.InternalOrganisation)
                .WithExecutedBy(this.InternalOrganisation)
                .WithTakenBy(internalOrganisation)
                .Build();

            this.Transaction.Derive();
            this.Transaction.Commit();

            var timeEntry = new TimeEntryBuilder(this.Transaction)
                .WithRateType(new RateTypes(this.Transaction).StandardRate)
                .WithFromDate(this.Transaction.Now())
                .WithWorkEffort(workTask)
                .Build();

            localAdmin.TimeSheetWhereWorker.AddTimeEntry(timeEntry);
            this.Transaction.Derive();

            workTask.Complete();
            this.Transaction.Derive();

            this.Transaction.SetUser(localAdmin);

            Assert.Equal(workTask.WorkEffortState, new WorkEffortStates(this.Transaction).Finished);

            var acl = new DatabaseAccessControl(this.Security, localAdmin)[workTask];
            Assert.True(acl.CanExecute(M.WorkTask.Revise));
        }

        [Fact]
        public void ReviseFinishedWorkTaskForCustomer()
        {
            var internalOrganisation = new Organisations(this.Transaction).Extent().First(o => o.IsInternalOrganisation);

            var localAdmin = new PersonBuilder(this.Transaction)
                .WithUserName("localadmin")
                .WithLastName("localAdmin")
                .Build();

            internalOrganisation.AddLocalAdministrator(localAdmin);
            new EmploymentBuilder(this.Transaction).WithEmployer(internalOrganisation).WithEmployee(localAdmin).Build();

            var userGroups = new UserGroups(this.Transaction);
            userGroups.Creators.AddMember(localAdmin);
            userGroups.LocalAdministratorsGlobal.AddMember(localAdmin);

            this.Transaction.Derive(true);

            var customer = new OrganisationBuilder(this.Transaction).WithName("Org1").Build();
            new CustomerRelationshipBuilder(this.Transaction).WithCustomer(customer).WithInternalOrganisation(internalOrganisation).Build();

            var workTask = new WorkTaskBuilder(this.Transaction).WithName("Activity").WithCustomer(customer).WithTakenBy(internalOrganisation).Build();

            this.Transaction.Derive();
            this.Transaction.Commit();

            var timeEntry = new TimeEntryBuilder(this.Transaction)
                .WithRateType(new RateTypes(this.Transaction).StandardRate)
                .WithFromDate(this.Transaction.Now())
                .WithThroughDate(this.Transaction.Now().AddHours(1))
                .WithWorkEffort(workTask)
                .Build();

            localAdmin.TimeSheetWhereWorker.AddTimeEntry(timeEntry);
            this.Transaction.Derive();

            workTask.Complete();
            this.Transaction.Derive(false);

            workTask.Invoice();
            this.Transaction.Derive(false);

            this.Transaction.SetUser(localAdmin);

            Assert.Equal(workTask.WorkEffortState, new WorkEffortStates(this.Transaction).Finished);

            var acl = new DatabaseAccessControl(this.Security, localAdmin)[workTask];
            Assert.False(acl.CanExecute(M.WorkTask.Revise));
        }
    }
}
