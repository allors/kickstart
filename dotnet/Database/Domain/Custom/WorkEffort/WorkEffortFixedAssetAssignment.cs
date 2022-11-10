// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WorkEffortFixedAssetAssignment.cs" company="Allors bvba">
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
    public partial class WorkEffortFixedAssetAssignment
    {
        public void AviationOnPostDerive(ObjectOnPostDerive method)
        {
            this.SecurityTokens = new[]
            {
                new SecurityTokens(this.Transaction()).DefaultSecurityToken,
            };

            if (this.ExistAssignment && this.Assignment.ExistTakenBy)
            {
                this.AddSecurityToken(this.Assignment.TakenBy.BlueCollarWorkerSecurityToken);
                this.AddSecurityToken(this.Assignment.TakenBy.StockManagerSecurityToken);
                this.AddSecurityToken(this.Assignment.TakenBy.LocalAdministratorSecurityToken);
                this.AddSecurityToken(this.Assignment.TakenBy.LocalEmployeeSecurityToken);
                this.AddSecurityToken(this.Assignment.TakenBy.LocalEmployeeSecurityToken);
            }

            if (this.ExistAssignment && this.Assignment.ExistCustomer && this.Assignment.Customer is Organisation customer)
            {
                this.AddSecurityToken(customer.ContactsSecurityToken);
            }
        }
    }
}
