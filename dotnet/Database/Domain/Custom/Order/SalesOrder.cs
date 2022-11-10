// <copyright file="SalesOrder.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

using System.Linq;

namespace Allors.Database.Domain
{
    public partial class SalesOrder
    {
        public void CustomOnPostDerive(ObjectOnPostDerive method)
        {
            this.SecurityTokens = new[]
            {
                new SecurityTokens(this.Transaction()).DefaultSecurityToken,
            };

            if (this.ExistTakenBy)
            {
                this.AddSecurityToken(this.TakenBy.LocalAdministratorSecurityToken);
                this.AddSecurityToken(this.TakenBy.LocalEmployeeSecurityToken);
                this.AddSecurityToken(this.TakenBy.LocalSalesAccountManagerSecurityToken);
            }
        }
    }
}
