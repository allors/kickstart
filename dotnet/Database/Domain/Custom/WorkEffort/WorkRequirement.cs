// <copyright file="WorkRequirement.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    public partial class WorkRequirement
    {
        public void CustomOnPostDerive(ObjectOnPostDerive method)
        {
            this.SecurityTokens = new[]
            {
                new SecurityTokens(this.strategy.Transaction).DefaultSecurityToken,
            };

            if (this.ExistServicedBy)
            {
                this.AddSecurityToken(this.ServicedBy.LocalAdministratorSecurityToken);
                this.AddSecurityToken(this.ServicedBy.LocalEmployeeSecurityToken);
            }

            if (this.Originator is Organisation customer)
            {
                this.AddSecurityToken(customer.ContactsSecurityToken);
            }
        }
    }
}
