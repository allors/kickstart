// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ProductQuote.cs" company="Allors bvba">
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
using System.Collections.Generic;
using System.Linq;

namespace Allors.Database.Domain
{
    public partial class SalesInvoice
    {
        public void CustomOnPostDerive(ObjectOnPostDerive method)
        {
            this.SecurityTokens = new[]
            {
                new SecurityTokens(this.Transaction()).DefaultSecurityToken,
            };

            if (this.ExistBilledFrom)
            {
                this.AddSecurityToken(this.BilledFrom?.LocalAdministratorSecurityToken);
                this.AddSecurityToken(this.BilledFrom?.LocalEmployeeSecurityToken);
                this.AddSecurityToken(this.BilledFrom?.LocalSalesAccountManagerSecurityToken);
            }
        }
    }
}