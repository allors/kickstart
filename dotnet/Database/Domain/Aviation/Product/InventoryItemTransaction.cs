// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SerialisedItem.cs" company="Allors bvba">
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

namespace Allors.Database.Domain
{
    public partial class InventoryItemTransaction
    {
        public void AviationOnPostDerive(ObjectOnPostDerive method)
        {
            this.SecurityTokens = new[]
            {
                new SecurityTokens(this.strategy.Transaction).DefaultSecurityToken,
            };

            if (this.ExistFacility)
            {
                this.AddSecurityToken(this.Facility.Owner?.LocalAdministratorSecurityToken);
                this.AddSecurityToken(this.Facility.Owner?.LocalEmployeeSecurityToken);
            }
        }
    }
}