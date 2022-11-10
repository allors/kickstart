// <copyright file="DemoTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

using Allors.Database.Domain;

namespace Allors.Database.Domain.Tests
{
    using Xunit;

    public class TestPopulationTests : DomainTest, IClassFixture<Fixture>
    {
        public TestPopulationTests(Fixture fixture) : base(fixture, false) { }

        [Fact]
        public void Demo()
        {
            var transaction = this.Transaction;
            var database = transaction.Database;

            var config = new Config();
            new Setup(database, config).Apply();

            transaction.Rollback();

            new Upgrade(transaction, null).Execute();

            transaction.Derive();
            transaction.Commit();

            new Allors.TestPopulation(transaction).ForDemo();

            transaction.Derive();
            transaction.Commit();
        }

        [Fact]
        public void E2E()
        {
            var transaction = this.Transaction;
            var database = transaction.Database;

            var config = new Config();
            new Setup(database, config).Apply();

            transaction.Rollback();

            new Upgrade(transaction, null).Execute();

            transaction.Derive();
            transaction.Commit();

            new Allors.TestPopulation(transaction).ForE2E();

            transaction.Derive();
            transaction.Commit();
        }
    }
}
