// <copyright file="OwnerTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain.Tests
{
    using System.Linq;
    using Xunit;

    public class OwnerSecurityTests : DomainTest, IClassFixture<Fixture>
    {
        public OwnerSecurityTests(Fixture fixture) : base(fixture) { }

        public override Config Config => new Config { SetupSecurity = true };

        [Fact]
        public void OwnerEditPerson()
        {
            var internalOrganisation = new Organisations(this.Transaction).Extent().First(o => o.IsInternalOrganisation);

            var person = internalOrganisation.EmploymentsWhereEmployer.First().Employee;

            this.Transaction.SetUser(person);

            var acl = new DatabaseAccessControl(this.Security, person)[person];
            Assert.True(acl.CanWrite(M.Person.UserPasswordHash));
            Assert.True(acl.CanWrite(M.Person.InUserPassword));
            Assert.True(acl.CanWrite(M.Person.InExistingUserPassword));
        }
    }
}
