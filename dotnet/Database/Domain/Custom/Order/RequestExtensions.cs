// <copyright file="RequestExtensions.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

using Allors.Database.Meta;

namespace Allors.Database.Domain
{
    using System.Linq;

    public static partial class RequestExtensions
    {
        public static void CustomOnPostDerive(this Request @this, ObjectOnPostDerive method)
        {
            @this.SecurityTokens = new[]
            {
                new SecurityTokens(@this.Strategy.Transaction).DefaultSecurityToken,
            };

            if (@this.ExistRecipient)
            {
                @this.AddSecurityToken(@this.Recipient.LocalAdministratorSecurityToken);
                @this.AddSecurityToken(@this.Recipient.LocalEmployeeSecurityToken);
                @this.AddSecurityToken(@this.Recipient.LocalSalesAccountManagerSecurityToken);
            }
        }
    }
}
