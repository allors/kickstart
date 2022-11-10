// <copyright file="FacilityTypes.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;

    public partial class FacilityTypes
    {
        public static readonly Guid WorkshopId = new Guid("07d554f3-421b-47f3-915a-60b3639f7371");

        public FacilityType Workshop => this.Cache[WorkshopId];

        protected override void AviationSetup(Setup setup)
        {
            var dutchLocale = new Locales(this.Transaction).DutchNetherlands;

            var merge = this.Cache.Merger().Action();
            var localisedName = new LocalisedTextAccessor(this.Meta.LocalisedNames);

            merge(WorkshopId, v =>
            {
                v.Name = "Workshop";
                localisedName.Set(v, dutchLocale, "Werkplaats");
                v.IsActive = true;
            });
        }
    }
}
