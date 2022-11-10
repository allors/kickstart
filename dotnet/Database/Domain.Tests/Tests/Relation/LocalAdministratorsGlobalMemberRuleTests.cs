// <copyright file="PersonLocalAdministratorsGlobalRuleTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

using Xunit;

namespace Allors.Database.Domain.Tests
{
    public class LocalAdministratorsGlobalMemberRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public LocalAdministratorsGlobalMemberRuleTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void AddLocalAdministratorDeriveLocalAdministratorsGlobalMembers()
        {
            this.InternalOrganisation.AddLocalAdministrator(this.Employee);
            this.Transaction.Derive(false);

            Assert.Contains(this.Employee, new UserGroups(this.Transaction).LocalAdministratorsGlobal.Members);
        }

        [Fact]
        public void RemoveLocalAdministratorDeriveLocalAdministratorsGlobalMembers()
        {
            this.InternalOrganisation.AddLocalAdministrator(this.Employee);
            this.Transaction.Derive(false);

            Assert.Contains(this.Employee, new UserGroups(this.Transaction).LocalAdministratorsGlobal.Members);

            this.InternalOrganisation.RemoveLocalAdministrator(this.Employee);
            this.Transaction.Derive(false);

            Assert.DoesNotContain(this.Employee, new UserGroups(this.Transaction).LocalAdministratorsGlobal.Members);
        }
    }
}
