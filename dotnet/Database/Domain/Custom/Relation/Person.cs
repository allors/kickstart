// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Person.cs" company="Allors bvba">
//   Copyright 2002-2012 Allors bvba.
// Dual Licensed under
//   a) the General Public Licence v3 (GPL)
//   b) the Allors License
// The GPL License is included in the file gpl.txt.
// The Allors License is an addendum to your contract.
// Allors Applications is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// For more information visit http://www.allors.com/legal
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Linq;

namespace Allors.Database.Domain
{
    public partial class Person
    {
        public void CustomOnPostDerive(ObjectOnPostDerive method)
        {
            var transaction = this.Strategy.Transaction;
            var internalOrganisations = transaction.Extent<Organisation>().Where(v => v.IsInternalOrganisation).ToArray();

            this.SecurityTokens = new[]
            {
                new SecurityTokens(this.Strategy.Transaction).DefaultSecurityToken, this.OwnerSecurityToken
            };

            // employees have access to each other: needed to display modifiedby name etc.
            foreach (Employment employment in this.EmploymentsWhereEmployee)
            {
                if (employment.Employer.IsInternalOrganisation)
                {
                    foreach (InternalOrganisation @this in internalOrganisations)
                    {
                        this.AddSecurityToken(@this.LocalEmployeeSecurityToken);
                    }
                }
            }

            // All employees have access to all people via local employee security
            // All local administrators have access to all people via local administrator security
            foreach (var @this in internalOrganisations)
            {
                this.AddSecurityToken(@this.LocalEmployeeSecurityToken);
                this.AddSecurityToken(@this.LocalAdministratorSecurityToken);
            }
        }
    }
}