// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WorkEffortInventoryAssignment.cs" company="Allors bvba">
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
    public partial class WorkEffortInventoryAssignment
    {
        public void CustomOnPostDerive(ObjectOnPostDerive method)
        {
            this.SecurityTokens = new[]
            {
                new SecurityTokens(this.Transaction()).DefaultSecurityToken,
                this.Assignment?.TakenBy?.LocalAdministratorSecurityToken,
                this.Assignment?.TakenBy?.LocalEmployeeSecurityToken,
                this.Assignment?.TakenBy?.StockManagerSecurityToken,
                this.Assignment?.TakenBy?.LocalSalesAccountManagerSecurityToken,
            };
        }
    }
}