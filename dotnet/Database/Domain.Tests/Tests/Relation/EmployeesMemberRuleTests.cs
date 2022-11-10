// <copyright file="EmployeesMemberRuleTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

using Xunit;

namespace Allors.Database.Domain.Tests
{
    public class EmployeesMemberRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public EmployeesMemberRuleTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void AddLocalEmployeeDeriveEmployeesMembers()
        {
            this.InternalOrganisation.AddLocalAdministrator(this.Employee);
            this.Transaction.Derive(false);

            Assert.Contains(this.Employee, new UserGroups(this.Transaction).Employees.Members);
        }

        [Fact]
        public void AddAdministratorDeriveEmployeesMembers()
        {
            new UserGroups(this.Transaction).Administrators.AddMember(this.Employee);
            this.Transaction.Derive(false);

            Assert.Contains(this.Employee, new UserGroups(this.Transaction).Employees.Members);
        }

        [Fact]
        public void RemoveAdministratorDeriveEmployeesMembers()
        {
            var person = new PersonBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            new UserGroups(this.Transaction).Administrators.AddMember(person);
            this.Transaction.Derive(false);

            Assert.Contains(person, new UserGroups(this.Transaction).Employees.Members);

            new UserGroups(this.Transaction).Administrators.RemoveMember(person);
            this.Transaction.Derive(false);

            Assert.DoesNotContain(person, new UserGroups(this.Transaction).Employees.Members);
        }
    }
}
