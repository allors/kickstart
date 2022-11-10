// <copyright file="PersonLocalAdministratorsGlobalRuleTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

using Xunit;

namespace Allors.Database.Domain.Tests
{
    public class SalesAccountMangersGlobalMemberRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public SalesAccountMangersGlobalMemberRuleTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void AddLocalSalesAccountMangerDeriveSalesAccountMangerGlobalMembers()
        {
            this.InternalOrganisation.AddLocalSalesAccountManager(this.Employee);
            this.Transaction.Derive(false);

            Assert.Contains(this.Employee, new UserGroups(this.Transaction).SalesAccountManagersGlobal.Members);
        }

        [Fact]
        public void RemoveSalesAccountMangerDeriveSalesAccountMangersGlobalMembers()
        {
            this.InternalOrganisation.AddLocalSalesAccountManager(this.Employee);
            this.Transaction.Derive(false);

            Assert.Contains(this.Employee, new UserGroups(this.Transaction).SalesAccountManagersGlobal.Members);

            this.InternalOrganisation.RemoveLocalSalesAccountManager(this.Employee);
            this.Transaction.Derive(false);

            Assert.DoesNotContain(this.Employee, new UserGroups(this.Transaction).SalesAccountManagersGlobal.Members);
        }
    }
}
