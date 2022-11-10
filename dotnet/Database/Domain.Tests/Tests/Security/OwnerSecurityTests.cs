// <copyright file="OwnerTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain.Tests
{
    using Xunit;

    public class OwnerSecurityTests : DomainTest, IClassFixture<Fixture>
    {
        public OwnerSecurityTests(Fixture fixture) : base(fixture) { }

        public override Config Config => new Config { SetupSecurity = true };

        [Fact]
        public void EditOwnSecurityRoles()
        {
            var jane = new PersonBuilder(this.Transaction)
                .WithFirstName("Jane")
                .WithLastName("Doe")
                .WithUserName("jane")
                .Build();

            this.Transaction.Derive();

            this.Transaction.SetUser(jane);

            var acl = new DatabaseAccessControl(this.Security, jane)[jane];
            Assert.True(acl.CanWrite(M.Person.UserPasswordHash));
            Assert.True(acl.CanWrite(M.Person.InUserPassword));
            Assert.True(acl.CanWrite(M.Person.InExistingUserPassword));
        }

        [Fact]
        public void EditOtherSecurityRoles()
        {
            var jane = new PersonBuilder(this.Transaction)
                .WithFirstName("Jane")
                .WithLastName("Doe")
                .WithUserName("jane")
                .Build();

            var john = new PersonBuilder(this.Transaction)
                .WithFirstName("John")
                .WithLastName("Doe")
                .WithUserName("john")
                .Build();

            this.Transaction.Derive();

            this.Transaction.SetUser(jane);

            var acl = new DatabaseAccessControl(this.Security, jane)[john];
            Assert.False(acl.CanWrite(M.Person.UserPasswordHash));
            Assert.False(acl.CanWrite(M.Person.InUserPassword));
            Assert.False(acl.CanWrite(M.Person.InExistingUserPassword));
        }

    }
}
