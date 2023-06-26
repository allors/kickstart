// <copyright file="DomainTest.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the DomainTest type.</summary>

using Allors.Database.Configuration.Derivations.Default;
using Allors.Database.Meta;
using Allors.Database.Services;

namespace Allors.Database.Domain.Tests
{
    using System;
    using System.Globalization;
    using System.Linq;
    using Adapters.Memory;
    using Domain;
    using Configuration;
    using Database;
    using TestPopulation;
    using Allors.Database.Derivations;

    public class DomainTest : IDisposable
    {
        public DomainTest(Fixture fixture, bool populate = true)
        {
            var database = new Database(
                new TestDatabaseServices(fixture.Engine, null),
                new Configuration
                {
                    ObjectFactory = new ObjectFactory(fixture.M, typeof(User)),
                });

            this.M = database.Services.Get<MetaPopulation>();

            this.Setup(database, populate);
        }

        public MetaPopulation M { get; }

        public virtual Config Config { get; } = new Config { SetupSecurity = false };

        public ITransaction Transaction { get; private set; }

        public ITime Time => this.Transaction.Database.Services.Get<ITime>();

        public ISecurity Security => this.Transaction.Database.Services.Get<ISecurity>();

        public TimeSpan? TimeShift
        {
            get => this.Time.Shift;

            set => this.Time.Shift = value;
        }

        protected Person Administrator => (Person)new UserGroups(this.Transaction).Administrators.Members.First();

        public void Dispose()
        {
            this.Transaction.Rollback();
            this.Transaction = null;
        }

        protected IValidation Derive() => this.Transaction.Derive(false, true);

        protected void Setup(IDatabase database, bool populate)
        {
            database.Init();

            this.Transaction = database.CreateTransaction();

            if (populate)
            {
                this.Populate(database);
            }
        }

        private void Populate(IDatabase database)
        {
            CultureInfo.CurrentCulture = new CultureInfo("en-GB");
            CultureInfo.CurrentUICulture = new CultureInfo("en-GB");

            new Setup(database, this.Config).Apply();

            this.Transaction.Rollback();

            var administrator = new People(this.Transaction).FindBy(M.Person.UserName, "JaneDoe");

            if (administrator == null)
            {
                administrator = new PersonBuilder(this.Transaction)
                    .WithFirstName("Jane")
                    .WithLastName("Doe")
                    .WithUserName("JaneDoe")
                    .Build();

                new UserGroups(this.Transaction).Administrators.AddMember(administrator);
            }

            this.Transaction.Services.Get<IUserService>().User = administrator;

            var singleton = this.Transaction.GetSingleton();

            this.Transaction.Derive();
            this.Transaction.Commit();
        }

        private Person GetPersonByUserName(string userName) => new People(this.Transaction).FindBy(this.M.User.UserName, userName);
    }
}
