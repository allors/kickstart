// <copyright file="MaintenanceAgreementTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

using System.Linq;
using Xunit;

namespace Allors.Database.Domain.Tests
{
    public class MaintenanceAgreementTests : DomainTest, IClassFixture<Fixture>
    {
        public MaintenanceAgreementTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedCustomerRelationshipInternalOrganisationDeriveInternalOrganisation()
        {
            var agreeement = new MaintenanceAgreementBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var customerRelationship = new CustomerRelationshipBuilder(this.Transaction)
                .WithCustomer(this.Customer)
                .WithAgreement(agreeement)
                .Build();
            this.Transaction.Derive(false);

            customerRelationship.InternalOrganisation = this.InternalOrganisation;
            this.Transaction.Derive(false);

            Assert.Equal(this.InternalOrganisation, agreeement.InternalOrganisation);
        }

        [Fact]
        public void ChangedCustomerRelationshipCustomerNameDeriveCustomerName()
        {
            var customer = new OrganisationBuilder(this.Transaction).Build();
            var agreeement = new MaintenanceAgreementBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            new CustomerRelationshipBuilder(this.Transaction)
                .WithCustomer(customer)
                .WithAgreement(agreeement)
                .Build();
            this.Transaction.Derive(false);

            customer.Name = "changed";
            this.Transaction.Derive(false);

            Assert.Contains("changed", agreeement.CustomerName);
        }

        [Fact]
        public void ChangedWorkEffortTypeDeriveDescription()
        {
            var workEffortType = new WorkEffortTypeBuilder(this.Transaction).WithDescription("description").Build();
            this.Transaction.Derive(false);

            var agreeement = new MaintenanceAgreementBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            agreeement.WorkEffortType = workEffortType;
            this.Transaction.Derive(false);

            Assert.Contains(workEffortType.Description, agreeement.Description);
        }

        [Fact]
        public void ChangedDescriptionDeriveDescription()
        {
            var workEffortType = new WorkEffortTypeBuilder(this.Transaction).WithDescription("workefforttypesdescription").Build();
            this.Transaction.Derive(false);

            var agreeement = new MaintenanceAgreementBuilder(this.Transaction)
                .WithWorkEffortType(workEffortType)
                .WithDescription("agreementdesc")
                .Build();
            this.Transaction.Derive(false);

            Assert.Contains("agreementdesc", agreeement.Description);

            agreeement.RemoveDescription();
            this.Transaction.Derive(false);

            Assert.Contains("workefforttypesdescription", agreeement.Description);
        }
    }

    [Trait("Category", "Security")]
    public class MaintenanceAgreementDeniedPermissionRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public MaintenanceAgreementDeniedPermissionRuleTests(Fixture fixture) : base(fixture) => this.deleteRevocation = new Revocations(this.Transaction).MaintenanceAgreementDeleteRevocation;

        public override Config Config => new Config { SetupSecurity = true };

        private readonly Revocation deleteRevocation;

        [Fact]
        public void OnChangedWorkTaskMaintenanceAgreementDeriveDeletePermissionAllowed()
        {
            var agreement = new MaintenanceAgreementBuilder(this.Transaction).Build();
            var worktask = new WorkTaskBuilder(this.Transaction).WithMaintenanceAgreement(agreement).Build();
            this.Transaction.Derive(false);

            worktask.RemoveMaintenanceAgreement();
            this.Transaction.Derive(false);

            Assert.DoesNotContain(this.deleteRevocation, agreement.Revocations);
        }

        [Fact]
        public void OnChangedWorkTaskMaintenanceAgreementDeriveDeletePermissionDenied()
        {
            var agreement = new MaintenanceAgreementBuilder(this.Transaction).Build();
            var worktask = new WorkTaskBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            worktask.MaintenanceAgreement = agreement;
            this.Transaction.Derive(false);

            Assert.Contains(this.deleteRevocation, agreement.Revocations);
        }
    }
}
