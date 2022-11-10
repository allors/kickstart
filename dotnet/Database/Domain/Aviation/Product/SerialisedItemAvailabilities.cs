// <copyright file="SerialisedItemAvailabilities.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;

    public partial class SerialisedItemAvailabilities
    {
        public static readonly Guid SoldThirPartyId = new Guid("2276ccd7-74b2-48e6-a4ba-452d292ae23c");

        public SerialisedItemAvailability SoldThirParty => this.Cache[SoldThirPartyId];

        protected override void AviationSetup(Setup setup)
        {
            var merge = this.Cache.Merger().Action();

            merge(SoldThirPartyId, v =>
            {
                v.Name = "Sold by third party";
                v.IsActive = false;
            });
        }
    }
}
