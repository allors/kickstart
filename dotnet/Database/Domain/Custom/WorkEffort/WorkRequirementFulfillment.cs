// <copyright file="WorkRequirement.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    public partial class WorkRequirementFulfillment
    {
        public void CustomOnPostDerive(ObjectOnPostDerive method)
        {
            this.SecurityTokens = new[]
            {
                new SecurityTokens(this.strategy.Transaction).DefaultSecurityToken,
            };

            if (this.ExistFullfilledBy && this.FullfilledBy.ExistServicedBy)
            {
                this.AddSecurityToken(this.FullfilledBy.ServicedBy.LocalAdministratorSecurityToken);
                this.AddSecurityToken(this.FullfilledBy.ServicedBy.LocalEmployeeSecurityToken);
            }

            if (this.ExistFullfilledBy && this.FullfilledBy.Originator is Organisation customer)
            {
                this.AddSecurityToken(customer.ContactsSecurityToken);
            }
        }
    }
}
