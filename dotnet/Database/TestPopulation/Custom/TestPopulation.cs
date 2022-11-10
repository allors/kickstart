// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Setup.cs" company="Allors bvba">
//   Copyright 2002-2012 Allors bvba.
//
// Dual Licensed under
//   a) the General Public Licence v3 (GPL)
//   b) the Allors License
//
// The GPL License is included in the file gpl.txt.
// The Allors License is an addendum to your contract.
//
// Allors Applications is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// For more information visit http://www.allors.com/legal
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System.Linq;
using Allors.Database;
using Allors.Database.Domain;
using Allors.Database.Domain.TestPopulation;
using Allors.Database.Meta;

namespace Allors
{
    public partial class TestPopulation
    {
        private readonly ITransaction transaction;

        public TestPopulation(ITransaction transaction)
        {
            this.transaction = transaction;
        }

        public void ForDemo()
        {
            var m = this.transaction.Database.Services.Get<MetaPopulation>();

            var administrator = new PersonBuilder(transaction)
                .WithFirstName("Jane")
                .WithLastName("Doe")
                .WithUserName("administrator")
                .Build();

            new UserGroups(transaction).Administrators.AddMember(administrator);

            transaction.Services.Get<IUserService>().User = administrator;


            this.transaction.Derive();
            this.transaction.Commit();
        }

        public void ForE2E()
        {
            var administrator = new PersonBuilder(transaction)
                .WithFirstName("Jane")
                .WithLastName("Doe")
                .WithUserName("administrator")
                .Build();

            new UserGroups(transaction).Administrators.AddMember(administrator);

            transaction.Services.Get<IUserService>().User = administrator;

            this.transaction.Derive();
            this.transaction.Commit();
        }
    }
}
