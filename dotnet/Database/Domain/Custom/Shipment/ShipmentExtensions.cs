// <copyright file="ShipmentExtensions.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    public static partial class ShipmentExtensions
    {
        public static void CustomOnPostDerive(this Shipment @this, ObjectOnPostDerive method)
        {
            @this.SecurityTokens = new[]
            {
                new SecurityTokens(@this.Strategy.Transaction).DefaultSecurityToken,
            };

            if (@this.ExistShipFromParty && @this.ShipFromParty is InternalOrganisation from)
            {
                @this.AddSecurityToken(from.LocalAdministratorSecurityToken);
                @this.AddSecurityToken(from.LocalEmployeeSecurityToken);
            }

            if (@this.ExistShipToParty && @this.ShipToParty is InternalOrganisation to)
            {
                @this.AddSecurityToken(to.LocalAdministratorSecurityToken);
                @this.AddSecurityToken(to.LocalEmployeeSecurityToken);
            }
        }
    }
}
