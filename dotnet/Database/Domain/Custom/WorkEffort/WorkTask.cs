// <copyright file="WorkTask.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

using System.Collections.Generic;
using System.Linq;

namespace Allors.Database.Domain
{
    public partial class WorkTask
    {
        public void CustomOnPostDerive(ObjectOnPostDerive method)
        {
            var derivation = method.Derivation;

            this.SecurityTokens = new[]
            {
                new SecurityTokens(this.strategy.Transaction).DefaultSecurityToken,
            };

            if (this.ExistTakenBy)
            {
                this.AddSecurityToken(this.TakenBy.BlueCollarWorkerSecurityToken);
                this.AddSecurityToken(this.TakenBy.StockManagerSecurityToken);
                this.AddSecurityToken(this.TakenBy.LocalAdministratorSecurityToken);
                this.AddSecurityToken(this.TakenBy.LocalEmployeeSecurityToken);
                this.AddSecurityToken(this.TakenBy.LocalSalesAccountManagerSecurityToken);
            }

            if (this.Customer is Organisation customer)
            {
                this.AddSecurityToken(customer.ContactsSecurityToken);
            }

            this.ResetPrintWorkerDocument();
        }
    }
}