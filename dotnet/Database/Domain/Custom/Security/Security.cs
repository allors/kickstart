
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Security.cs" company="Allors bvba">
//   Copyright 2002-2016 Allors bvba.
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

namespace Allors.Database.Domain
{
    using Allors.Database.Meta;
    using System.Collections.Generic;

    public partial class Security
    {
        private void CustomOnPreSetup()
        {
            var m = this.transaction.Database.Services.Get<MetaPopulation>();

            var security = new Security(this.transaction);

            var full = new[] { Operations.Read, Operations.Write, Operations.Execute };
            var read = new[] { Operations.Read };

            foreach (ObjectType @class in this.transaction.Database.MetaPopulation.DatabaseClasses)
            {
                // global
                security.GrantAdministrator(@class, full);
                security.GrantCreator(@class, full);

                if (@class.Equals(m.Singleton))
                {
                    security.Grant(Roles.GuestId, @class, m.Singleton.DefaultLocale, Operations.Read);
                }
                else if (@class.Equals(m.Person))
                {
                    security.Grant(Roles.OwnerId, @class, m.Person.FirstName, Operations.Read, Operations.Write);
                    security.Grant(Roles.OwnerId, @class, m.Person.MiddleName, Operations.Read, Operations.Write);
                    security.Grant(Roles.OwnerId, @class, m.Person.LastName, Operations.Read, Operations.Write);
                    security.Grant(Roles.OwnerId, @class, m.Person.UserPasswordHash, Operations.Read, Operations.Write);
                    security.Grant(Roles.OwnerId, @class, m.Person.InUserPassword, Operations.Read, Operations.Write);
                    security.Grant(Roles.OwnerId, @class, m.Person.InExistingUserPassword, Operations.Read, Operations.Write);
                }
            }
        }

        private void CustomOnPostSetup()
        {
        }
    }
}