// <copyright file="DemoTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

using Allors.Database.Domain;

namespace Allors.Database.Domain.Tests
{
    using Xunit;

    public class PopulationTests : DomainTest, IClassFixture<Fixture>
    {
        public PopulationTests(Fixture fixture) : base(fixture, false) { }

        [Fact]
        public void TestDemo()
        {
            var transaction = this.Transaction;
            var database = transaction.Database;

            var config = new Config();
            new Setup(database, config).Apply();

            transaction.Rollback();

            new Upgrade(transaction, null).Execute();

            transaction.Derive();
            transaction.Commit();

            new OldSetup(transaction).Demo();

            transaction.Derive();
            transaction.Commit();
        }

        [Fact]
        public void TestEnd2End()
        {
            var transaction = this.Transaction;
            var database = transaction.Database;

            var config = new Config();
            new Setup(database, config).Apply();

            transaction.Rollback();

            new Upgrade(transaction, null).Execute();

            transaction.Derive();
            transaction.Commit();

            new OldSetup(transaction).End2End();

            transaction.Derive();
            transaction.Commit();
        }

        [Fact]
        public void TestGatsby()
        {
            var transaction = this.Transaction;
            var database = transaction.Database;

            var config = new Config();
            new Setup(database, config).Apply();

            transaction.Rollback();

            new Upgrade(transaction, null).Execute();

            transaction.Derive();
            transaction.Commit();

            new GatsbySetup(transaction).Apply();

            transaction.Derive();
            transaction.Commit();
        }
    }
}
