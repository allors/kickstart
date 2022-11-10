// <copyright file="Ownerships.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;

    public partial class Ownerships
    {
        private static readonly Guid RepairAndmaintenanceId = new Guid("1dc742cc-d4ef-49b7-8e77-38ace9eae2df");

        public Ownership RepairAndmaintenance => this.Cache[RepairAndmaintenanceId];

        protected override void AviationSetup(Setup setup)
        {
            var dutchLocale = new Locales(this.Transaction).DutchNetherlands;

            var merge = this.Cache.Merger().Action();
            var localisedName = new LocalisedTextAccessor(this.Meta.LocalisedNames);

            merge(RepairAndmaintenanceId, v =>
            {
                v.Name = "R&M";
                localisedName.Set(v, dutchLocale, "R&M");
                v.IsActive = true;
            });
        }
    }
}
