// <copyright file="ProductQuoteApproverTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain.Tests
{
    using Allors;
    using Xunit;

    public class ProductQuoteApproverTests : DomainTest, IClassFixture<Fixture>
    {
        public ProductQuoteApproverTests(Fixture fixture) : base(fixture) { }
        
        [Fact]
        public void UserGroup()
        {
            var organisation = new OrganisationBuilder(this.Transaction).WithName("organisation").WithIsInternalOrganisation(true).Build();
            this.Transaction.Derive(true);

            Assert.True(organisation.ExistProductQuoteApproverUserGroup);

            organisation.RemoveProductQuoteApproverUserGroup();
            this.Transaction.Derive();

            Assert.True(organisation.ExistProductQuoteApproverUserGroup);
        }

        [Fact]
        public void Grant()
        {
            var organisation = new OrganisationBuilder(this.Transaction).WithName("organisation").WithIsInternalOrganisation(true).Build();
            this.Transaction.Derive(true);

            Assert.True(organisation.ExistProductQuoteApproverGrant);
            Assert.Equal(new Roles(this.Transaction).ProductQuoteApprover, organisation.ProductQuoteApproverGrant.Role);
            Assert.Contains(organisation.ProductQuoteApproverUserGroup, organisation.ProductQuoteApproverGrant.SubjectGroups);

            organisation.RemoveProductQuoteApproverGrant();

            this.Transaction.Derive(true);

            Assert.True(organisation.ExistProductQuoteApproverGrant);
            Assert.Equal(new Roles(this.Transaction).ProductQuoteApprover, organisation.ProductQuoteApproverGrant.Role);
            Assert.Contains(organisation.ProductQuoteApproverUserGroup, organisation.ProductQuoteApproverGrant.SubjectGroups);
        }

        [Fact]
        public void SecurityToken()
        {
            var organisation = new OrganisationBuilder(this.Transaction).WithName("organisation").WithIsInternalOrganisation(true).Build();
            this.Transaction.Derive(true);

            Assert.True(organisation.ExistProductQuoteApproverSecurityToken);
            Assert.Contains(organisation.ProductQuoteApproverGrant, organisation.ProductQuoteApproverSecurityToken.Grants);
        }

        [Fact]
        public void ProductQuoteApprovers()
        {
            var organisation = new OrganisationBuilder(this.Transaction).WithName("organisation").WithIsInternalOrganisation(true).Build();
            this.Transaction.Derive(true);

            var approver = new PersonBuilder(this.Transaction)
                .WithUserName("approver")
                .WithFirstName("productquote")
                .WithLastName("approver")
                .Build();

            organisation.AddProductQuoteApprover(approver);

            this.Transaction.Derive(true);

            Assert.Contains(approver, organisation.ProductQuoteApproverUserGroup.Members);
        }
    }
}
