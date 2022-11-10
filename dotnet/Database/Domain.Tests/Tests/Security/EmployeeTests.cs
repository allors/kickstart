// <copyright file="EmployeeTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain.Tests
{
    using System.Linq;
    using Allors;
    using Xunit;


    public class EmployeeSecurityTests : DomainTest, IClassFixture<Fixture>
    {
        public EmployeeSecurityTests(Fixture fixture) : base(fixture) { }

        public override Config Config => new Config { SetupSecurity = true };

        [Fact]
        public void Media()
        {
            var userGroup = new UserGroups(this.Transaction).Employees;
            var employee = userGroup.Members.First();

            var media = new MediaBuilder(this.Transaction).WithInFileName("doc1.en.pdf").Build();

            this.Transaction.SetUser(employee);

            var acl = new DatabaseAccessControl(this.Security, employee)[media];
            Assert.True(acl.CanRead(M.Media.Name));
            Assert.True(acl.CanWrite(M.Media.Name));
            Assert.True(acl.CanRead(M.Media.InType));
            Assert.True(acl.CanWrite(M.Media.InType));
            Assert.True(acl.CanRead(M.Media.InData));
            Assert.True(acl.CanWrite(M.Media.InData));
            Assert.True(acl.CanRead(M.Media.InDataUri));
            Assert.True(acl.CanWrite(M.Media.InDataUri));
            Assert.True(acl.CanRead(M.Media.InFileName));
            Assert.True(acl.CanWrite(M.Media.InFileName));
            Assert.True(acl.CanRead(M.Media.MediaContent));
            Assert.True(acl.CanWrite(M.Media.MediaContent));
            Assert.True(acl.CanExecute(M.Media.Delete));
        }

        [Fact]
        public void LocalisedText()
        {
            var userGroup = new UserGroups(this.Transaction).Employees;
            var employee = userGroup.Members.First();

            var localisedText = new LocalisedTextBuilder(this.Transaction).WithText("text").WithLocale(this.Transaction.GetSingleton().AdditionalLocales.First()).Build();

            this.Transaction.SetUser(employee);

            var acl = new DatabaseAccessControl(this.Security, employee)[localisedText];
            Assert.True(acl.CanRead(M.LocalisedText.Text));
            Assert.True(acl.CanWrite(M.LocalisedText.Text));
        }

        [Fact]
        public void ExchangeRate()
        {
            var user = new PersonBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            this.InternalOrganisation.AddLocalEmployee(user);
            this.Transaction.Derive(false);

            var exchangeRate = new ExchangeRateBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);
            this.Transaction.Commit();

            this.Transaction.SetUser(user);

            var acl = new DatabaseAccessControl(this.Security, user)[exchangeRate];
            Assert.True(acl.CanRead(M.ExchangeRate.Rate));
            Assert.False(acl.CanWrite(M.ExchangeRate.Rate));
        }
    }
}
