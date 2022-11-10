// <copyright file="CreatorsMemberRuleTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

using Xunit;

namespace Allors.Database.Domain.Tests
{
    public class CreatorsMemberRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public CreatorsMemberRuleTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void AddLocalEmployeeDeriveCreatorsMembers()
        {
            this.InternalOrganisation.AddLocalAdministrator(this.Employee);
            this.Transaction.Derive(false);

            Assert.Contains(this.Employee, new UserGroups(this.Transaction).Creators.Members);
        }

        [Fact]
        public void RemoveLocalEmployeeDeriveCreatorsMembers()
        {
            var person = new PersonBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            this.InternalOrganisation.AddLocalAdministrator(person);
            this.Transaction.Derive(false);

            Assert.Contains(person, new UserGroups(this.Transaction).Creators.Members);

            this.InternalOrganisation.RemoveLocalAdministrator(person);
            this.InternalOrganisation.RemoveLocalEmployee(person);
            this.Transaction.Derive(false);

            Assert.DoesNotContain(person, new UserGroups(this.Transaction).Creators.Members);
        }

        [Fact]
        public void AddAdministratorDeriveCreatorsMembers()
        {
            new UserGroups(this.Transaction).Administrators.AddMember(this.Employee);
            this.Transaction.Derive(false);

            Assert.Contains(this.Employee, new UserGroups(this.Transaction).Creators.Members);
        }

        [Fact]
        public void RemoveAdministratorDeriveCreatorsMembers()
        {
            var person = new PersonBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            new UserGroups(this.Transaction).Administrators.AddMember(person);
            this.Transaction.Derive(false);

            Assert.Contains(person, new UserGroups(this.Transaction).Creators.Members);

            new UserGroups(this.Transaction).Administrators.RemoveMember(person);
            this.Transaction.Derive(false);

            Assert.DoesNotContain(person, new UserGroups(this.Transaction).Creators.Members);
        }
    }
}
