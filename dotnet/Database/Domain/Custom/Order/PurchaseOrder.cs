// <copyright file="PurchaseOrder.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    public partial class PurchaseOrder
    {
        public void CustomOnPostDerive(ObjectOnPostDerive method)
        {
            this.SecurityTokens = new[]
            {
                new SecurityTokens(this.Transaction()).DefaultSecurityToken,
                this.OrderedBy?.PurchaseOrderApproverLevel1SecurityToken,
                this.OrderedBy?.PurchaseOrderApproverLevel2SecurityToken,
                this.OrderedBy?.LocalAdministratorSecurityToken,
                this.OrderedBy?.StockManagerSecurityToken,
                this.OrderedBy?.LocalEmployeeSecurityToken,
                this.OrderedBy?.LocalSalesAccountManagerSecurityToken,
            };
        }
    }
}
