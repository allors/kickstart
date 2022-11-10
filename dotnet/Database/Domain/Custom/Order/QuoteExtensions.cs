// <copyright file="QuoteExtensions.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

using Allors.Database.Meta;
using System.Linq;

namespace Allors.Database.Domain
{
    public static partial class QuoteExtensions
    {
        public static void CustomOnPostDerive(this Quote @this, ObjectOnPostDerive method)
        {
            @this.SecurityTokens = new[]
            {
                new SecurityTokens(@this.Strategy.Transaction).DefaultSecurityToken,
            };

            if (@this.ExistIssuer)
            {
                @this.AddSecurityToken(@this.Issuer.ProductQuoteApproverSecurityToken);
                @this.AddSecurityToken(@this.Issuer.LocalAdministratorSecurityToken);
                @this.AddSecurityToken(@this.Issuer.LocalEmployeeSecurityToken);
                @this.AddSecurityToken(@this.Issuer.LocalSalesAccountManagerSecurityToken);
            }
        }
    }
}
